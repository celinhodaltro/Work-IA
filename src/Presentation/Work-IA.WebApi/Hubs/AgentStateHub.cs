using Microsoft.AspNetCore.SignalR;

namespace Work_IA.WebApi.Hubs;

public sealed class AgentStateHub : Hub
{
    public async Task JoinOffice()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "office");
    }

    public async Task LeaveOffice()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "office");
    }
}
