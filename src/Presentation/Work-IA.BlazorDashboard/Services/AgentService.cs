using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Work_IA.BlazorDashboard.Services;

public sealed class AgentService
{
    private readonly HttpClient _http;

    public AgentService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<AgentDto>> GetAgentsAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<AgentDto>>("api/agents") ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<AgentDto?> GetAgentAsync(Guid id)
    {
        try
        {
            return await _http.GetFromJsonAsync<AgentDto>($"api/agents/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<Guid?> CreateAgentAsync(string name, string title)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/agents", new { name, title });
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Guid>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<AgentCostSummary>> GetTokenCostsAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<AgentCostSummary>>("api/tokens/agents") ?? [];
        }
        catch
        {
            return [];
        }
    }
}

public sealed class AgentDto
{
    public Guid AgentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int CareerLevel { get; set; }
    public string Status { get; set; } = "Created";
    public int ExperiencePoints { get; set; }
    public DateTime JoinedAt { get; set; }
}

public sealed class AgentCostSummary
{
    public Guid AgentId { get; set; }
    public string AgentName { get; set; } = "";
    public string AgentTitle { get; set; } = "";
    public int TotalTokensIn { get; set; }
    public int TotalTokensOut { get; set; }
    public int TotalTokens => TotalTokensIn + TotalTokensOut;
    public decimal TotalCost { get; set; }
    public string MostUsedModel { get; set; } = "";
    public int TaskCount { get; set; }
}
