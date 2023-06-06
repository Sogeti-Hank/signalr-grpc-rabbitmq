using GrpcService;
using Microsoft.AspNetCore.SignalR;
using Shared.Messages;

namespace Web.Hubs
{
    public interface IPipelinePublisher
    {
        Task Publish(PipelineResponse message);
    }

    public class PipelinePublisher : IPipelinePublisher
    {
        private readonly IHubContext<PipelineHub> _hubContext;

        public PipelinePublisher(IHubContext<PipelineHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Publish(PipelineResponse message)
        {
            if (_hubContext.Clients is not null)
            {
                await _hubContext.Clients.Group(message.ProjectId).SendAsync("Receive", message);
            }
        }
    }
}
