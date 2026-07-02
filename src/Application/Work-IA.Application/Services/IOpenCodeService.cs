namespace Work_IA.Application.Services;

public sealed record OpenCodePrompt(string Template, Dictionary<string, string> Variables);

public sealed record OpenCodeResult(bool Success, string RawOutput, string? ExtractedJson, string? Error);

public interface IOpenCodeService
{
    Task<OpenCodeResult> ExecutePromptAsync(OpenCodePrompt prompt, CancellationToken ct = default);
}
