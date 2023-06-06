using Microsoft.AspNetCore.SignalR;
using Shared.Messages;

namespace Web.Hubs;


public class PipelineHub : Hub
{
    public async Task Subscribe(string projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, projectId);
    }

    public async Task Unsubscribe(string projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, projectId);
    }
}
