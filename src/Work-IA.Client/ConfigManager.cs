using System.Text.Json;

namespace Work_IA.Client;

public sealed class ConfigManager
{
    private readonly string _configPath;
    private AppConfig _config;

    public ConfigManager()
    {
        _configPath = Path.Combine(AppContext.BaseDirectory, "work-ia-config.json");
        _config = Load();
    }

    public AppConfig Config => _config;
    public bool IsConfigured => !string.IsNullOrEmpty(_config.AiProvider);

    public void SetProvider(string provider)
    {
        _config.AiProvider = provider;
        Save();
    }

    public void SetApiKey(string key)
    {
        _config.ApiKey = key;
        Save();
    }

    public void SetEndpoint(string endpoint)
    {
        _config.Endpoint = endpoint;
        Save();
    }

    public void MarkConfigured()
    {
        _config.IsSetupComplete = true;
        Save();
    }

    public void Reset()
    {
        _config = new AppConfig();
        if (File.Exists(_configPath))
            File.Delete(_configPath);
    }

    private AppConfig Load()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
        }
        catch { }
        return new AppConfig();
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configPath, json);
    }
}

public sealed class AppConfig
{
    public string AiProvider { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public bool IsSetupComplete { get; set; }
}
