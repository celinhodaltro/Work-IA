using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Services;

namespace Work_IA.Infrastructure.Services;

public sealed class OpenCodeService : IOpenCodeService
{
    private readonly ILogger<OpenCodeService> _logger;
    private readonly HttpClient _http;
    private readonly OpenCodeMode _mode;
    private readonly string? _apiUrl;
    private static readonly string? OpenCodePath = ResolveOpenCodePath();
    private static readonly JsonDocumentOptions JsonOptions = new() { AllowTrailingCommas = true };

    private const string AgentPromptTemplate = """
You are an AI agent profile generator. Create a detailed agent profile based on:

Name: {NAME}
Role: {TITLE}
Level: {LEVEL}

Generate a JSON object with this exact structure and NO other text:
{
  "name": "{NAME}",
  "title": "{TITLE}",
  "level": "{LEVEL}",
  "skills": ["skill1", "skill2", "skill3"],
  "personality": "brief personality description",
  "expertise": "area of expertise",
  "workStyle": "how this agent approaches work"
}
""";

    public OpenCodeService(ILogger<OpenCodeService> logger, string? apiUrl = null)
    {
        _logger = logger;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(130) };
        _apiUrl = apiUrl;

        if (!string.IsNullOrEmpty(apiUrl))
        {
            _mode = OpenCodeMode.Http;
        }
        else if (OpenCodePath is not null)
        {
            _mode = OpenCodeMode.Process;
            _logger.LogInformation("OpenCode found at {Path}", OpenCodePath);
        }
        else
        {
            _mode = OpenCodeMode.Disabled;
            _logger.LogWarning("OpenCode not found. Set OpenCode:ApiUrl in config to use HTTP mode.");
        }
    }

    public async Task<OpenCodeResult> ExecutePromptAsync(OpenCodePrompt prompt, CancellationToken ct = default)
    {
        try
        {
            var resolvedPrompt = ResolveTemplate(prompt);

            var response = _mode switch
            {
                OpenCodeMode.Process => await RunProcessAsync(resolvedPrompt, ct),
                OpenCodeMode.Http => await RunHttpAsync(resolvedPrompt, ct),
                _ => (false, "", "OpenCode not configured")
            };

            if (!response.Item1)
                return new OpenCodeResult(false, response.Item2, null, response.Item3);

            var extracted = ExtractJson(response.Item2);
            return new OpenCodeResult(true, response.Item2, extracted, null);
        }
        catch (OperationCanceledException)
        {
            return new OpenCodeResult(false, "", null, "Request cancelled or timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenCode execution failed");
            return new OpenCodeResult(false, "", null, ex.Message);
        }
    }

    private static string ResolveTemplate(OpenCodePrompt prompt)
    {
        var template = string.IsNullOrEmpty(prompt.Template) ? AgentPromptTemplate : prompt.Template;
        var result = template;
        foreach (var kv in prompt.Variables)
            result = result.Replace($"{{{kv.Key}}}", kv.Value);
        return result;
    }

    private async Task<(bool Success, string Output, string? Error)> RunProcessAsync(string prompt, CancellationToken ct)
    {
        var path = OpenCodePath ?? "opencode";
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(120));

        var startInfo = new ProcessStartInfo
        {
            FileName = path,
            ArgumentList = { "run", "--auto", "--format", "json", prompt },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync().WaitAsync(timeoutCts.Token);
        var error = await process.StandardError.ReadToEndAsync().WaitAsync(timeoutCts.Token);
        await process.WaitForExitAsync(timeoutCts.Token);

        if (!string.IsNullOrEmpty(error))
            return (true, output, null);

        var message = ExtractFinalMessage(output);
        return (true, message ?? output, null);
    }

    private async Task<(bool Success, string Output, string? Error)> RunHttpAsync(string prompt, CancellationToken ct)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(120));

        var request = new { prompt };
        var response = await _http.PostAsJsonAsync($"{_apiUrl}/api/chat", request, timeoutCts.Token);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>(timeoutCts.Token);
        var text = result.TryGetProperty("response", out var r) ? r.GetString()
            : result.TryGetProperty("content", out var c) ? c.GetString()
            : result.ToString();

        return (true, text ?? "", null);
    }

    private static string ExtractFinalMessage(string jsonEvents)
    {
        var lines = jsonEvents.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines.Reverse())
        {
            try
            {
                using var doc = JsonDocument.Parse(line, JsonOptions);
                var root = doc.RootElement;

                if (root.TryGetProperty("type", out var type) && type.GetString() == "message")
                {
                    if (root.TryGetProperty("content", out var content))
                    {
                        var text = content.GetString();
                        if (!string.IsNullOrWhiteSpace(text))
                            return text;
                    }
                    else if (root.TryGetProperty("message", out var msg) &&
                             msg.TryGetProperty("content", out var msgContent))
                    {
                        var text = msgContent.GetString();
                        if (!string.IsNullOrWhiteSpace(text))
                            return text;
                    }
                }
            }
            catch { }
        }

        return jsonEvents;
    }

    private static string? ExtractJson(string text)
    {
        var start = text.IndexOf('{');
        if (start < 0) return null;

        var depth = 0;
        var end = -1;
        for (int i = start; i < text.Length; i++)
        {
            if (text[i] == '{') depth++;
            else if (text[i] == '}') { depth--; if (depth == 0) { end = i + 1; break; } }
        }

        if (end < 0) return null;

        var candidate = text[start..end];
        try
        {
            JsonDocument.Parse(candidate, JsonOptions);
            return candidate;
        }
        catch { return null; }
    }

    private static string? ResolveOpenCodePath()
    {
        var npmDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "npm");

        var candidates = new List<string>();
        if (Directory.Exists(npmDir))
        {
            candidates.Add(Path.Combine(npmDir, "opencode.cmd"));
            candidates.Add(Path.Combine(npmDir, "opencode"));
        }

        foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            candidates.Add(Path.Combine(dir.Trim(), "opencode.cmd"));
            candidates.Add(Path.Combine(dir.Trim(), "opencode"));
        }

        return candidates.FirstOrDefault(File.Exists);
    }

    public void Dispose() => _http.Dispose();

    private enum OpenCodeMode { Process, Http, Disabled }
}
