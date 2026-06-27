# Adapter System

## Workspace Adapter Pattern

The adapter system allows AI Office OS to work with multiple IDEs and platforms via the `IWorkspaceAdapter` interface.

### Interface
```csharp
public interface IWorkspaceAdapter
{
    string PlatformName { get; }
    Task<IReadOnlyList<WorkspaceFile>> GetFilesAsync(string pattern = "*", ...);
    Task<WorkspaceFile?> ReadFileAsync(string path, ...);
    Task WriteFileAsync(string path, string content, ...);
    Task DeleteFileAsync(string path, ...);
    Task<bool> IsConnectedAsync(...);
}
```

### Current Adapters

| Adapter | Status | Description |
|---------|--------|-------------|
| **FileSystemWorkspaceAdapter** | ✅ Operational | Watches local file system via `FileSystemWatcher` |
| **OpenCodeAdapter** | ⚠️ Stub | Connects to OpenCode ecosystem — methods log but return empty results |
| **ClaudeCodeAdapter** | ⚠️ Stub | Connects to Claude Code CLI — methods log but return empty results |

### Plugin System
Adapters can be registered as plugins via `IAdapterPlugin`:

1. Implement `IAdapterPlugin` in a separate assembly
2. Implement `IWorkspaceAdapter` in the plugin
3. Drop the assembly in the plugins directory
4. `PluginLoader.LoadFromDirectory()` discovers and registers it

### Adding a New Adapter
1. Implement `IWorkspaceAdapter`
2. Create an `IAdapterPlugin` for auto-discovery
3. Register in `PluginLoader` or configure via DI
4. Configure in `appsettings.json`
