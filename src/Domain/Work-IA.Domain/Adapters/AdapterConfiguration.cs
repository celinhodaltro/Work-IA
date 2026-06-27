namespace Work_IA.Domain.Adapters;

public sealed class AdapterConfiguration
{
    public string Platform { get; set; } = "filesystem";
    public bool Enabled { get; set; } = true;
    public int Priority { get; set; } = 10;
    public Dictionary<string, string> Settings { get; set; } = [];
}
