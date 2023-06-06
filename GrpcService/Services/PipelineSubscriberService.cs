using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Shared.Messages;
using System.Threading.Channels;

namespace GrpcService.Services
{
    public class PipelineSubscriberService : PipelineSubscriber.PipelineSubscriberBase
    {
        private readonly Channel<PipelineMessage> _channel;

        public PipelineSubscriberService(Channel<PipelineMessage> channel)
        {
            _channel = channel;
        }

        public override async Task Subscribe(Empty empty, IServerStreamWriter<PipelineResponse> responseStream, ServerCallContext context)
        {
            var token = context.CancellationToken;

            while (await _channel.Reader.WaitToReadAsync(token))
            {
                var message = await _channel.Reader.ReadAsync(token);

                var response = new PipelineResponse
                {
                    ProjectId = message.ProjectId,
                    Status = message.Status
                };

                await responseStream.WriteAsync(response, token);
            }
        }
    }

}