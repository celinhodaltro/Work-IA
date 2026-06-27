using Microsoft.AspNetCore.SignalR.Client;
using Work_IA.Office.Models;

namespace Work_IA.Office.SignalR;

public sealed class OfficeHubClient : IAsyncDisposable
{
    private HubConnection? _connection;
    private readonly CancellationTokenSource _disposeCts = new();
    public List<VisualAgent> Agents { get; } = [];
    public event Action? OnStateChanged;

    public async Task ConnectAsync(string url = "http://localhost:5000/hub/agent-states", CancellationToken ct = default)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect([
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30)
            ])
            .Build();

        _connection.On<AgentStateDto>("AgentStateUpdated", state =>
        {
            var agent = Agents.FirstOrDefault(a => a.Id == state.Id);
            if (agent is null)
            {
                agent = new VisualAgent();
                Agents.Add(agent);
            }
            agent.Id = state.Id;
            agent.Name = state.Name;
            agent.Title = state.Title;
            agent.Status = state.Status;
            agent.Emotion = state.Emotion;
            agent.TargetX = state.PositionX;
            agent.TargetY = state.PositionY;
            agent.Level = state.Level;
            agent.Xp = state.Xp;
            agent.IsInMeeting = state.IsInMeeting;
            OnStateChanged?.Invoke();
        });

        _connection.On<List<AgentStateDto>>("AgentStates", states =>
        {
            Agents.Clear();
            foreach (var s in states)
            {
                Agents.Add(new VisualAgent
                {
                    Id = s.Id, Name = s.Name, Title = s.Title,
                    Status = s.Status, Emotion = s.Emotion,
                    TargetX = s.PositionX, TargetY = s.PositionY,
                    CurrentX = s.PositionX, CurrentY = s.PositionY,
                    Level = s.Level, Xp = s.Xp, IsInMeeting = s.IsInMeeting
                });
            }
            OnStateChanged?.Invoke();
        });

        await TryConnectWithRetryAsync(ct);
    }

    private async Task TryConnectWithRetryAsync(CancellationToken ct)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_disposeCts.Token, ct);
        var attempt = 0;

        while (!linkedCts.Token.IsCancellationRequested)
        {
            try
            {
                await _connection!.StartAsync(linkedCts.Token);
                return; // Conectou com sucesso!
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine($"Waiting for backend... ({ex.Message})");

                attempt++;
                var delay = TimeSpan.FromSeconds(Math.Min(30, Math.Pow(2, attempt)));
                var jitter = Random.Shared.NextDouble() * 0.5 + 0.75; // 0.75 a 1.25
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * jitter);

                try
                {
                    await Task.Delay(delay, linkedCts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _disposeCts.Cancel();
        _disposeCts.Dispose();
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}

public sealed class AgentStateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Title { get; set; } = "";
    public string Status { get; set; } = "";
    public string Emotion { get; set; } = "Neutral";
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public int Level { get; set; }
    public int Xp { get; set; }
    public bool IsInMeeting { get; set; }
}
