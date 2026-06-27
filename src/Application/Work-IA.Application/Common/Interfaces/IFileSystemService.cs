namespace Work_IA.Application.Common.Interfaces;

public interface IFileSystemService
{
    Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default);
    Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string path);
    Task<string[]> GetFilesAsync(string pattern, CancellationToken cancellationToken = default);
    string GetRelativePath(string fullPath);
}
