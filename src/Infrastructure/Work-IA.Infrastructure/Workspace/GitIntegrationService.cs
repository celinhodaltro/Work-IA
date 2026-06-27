using System.Diagnostics;
using Work_IA.Application.Common.Interfaces;

namespace Work_IA.Infrastructure.Workspace;

public sealed class GitIntegrationService : IGitIntegrationService
{
    private readonly string _workspacePath;

    public GitIntegrationService(string workspacePath)
    {
        _workspacePath = workspacePath;
    }

    public async Task<GitCommitInfo?> GetLastCommitAsync(CancellationToken cancellationToken = default)
    {
        var (output, _) = await RunGitCommandAsync("log --oneline -1", cancellationToken);

        if (string.IsNullOrEmpty(output))
            return null;

        var parts = output.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        return new GitCommitInfo
        {
            Hash = parts.Length > 0 ? parts[0] : string.Empty,
            Message = parts.Length > 1 ? parts[1] : string.Empty
        };
    }

    public async Task<string> GetCurrentBranchAsync(CancellationToken cancellationToken = default)
    {
        var (output, _) = await RunGitCommandAsync("rev-parse --abbrev-ref HEAD", cancellationToken);
        return output?.Trim() ?? "unknown";
    }

    public async Task<IReadOnlyList<string>> GetChangedFilesAsync(CancellationToken cancellationToken = default)
    {
        var (output, _) = await RunGitCommandAsync("status --porcelain", cancellationToken);

        if (string.IsNullOrEmpty(output))
            return [];

        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line[3..].Trim())
            .ToList();
    }

    private async Task<(string? output, string? error)> RunGitCommandAsync(string arguments, CancellationToken cancellationToken = default)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = _workspacePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            return (output, error);
        }
        catch
        {
            return (null, "Git not available");
        }
    }
}
