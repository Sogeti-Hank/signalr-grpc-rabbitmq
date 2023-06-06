using MassTransit;
using Shared.Messages;
using System.Threading.Channels;

namespace GrpcService.Consumers
{
    public class PipelineConsumer : IConsumer<PipelineMessage>
    {
        private readonly Channel<PipelineMessage> _channel;

        public PipelineConsumer(Channel<PipelineMessage> channel)
        {
            _channel = channel;
        }

        public async Task Consume(ConsumeContext<PipelineMessage> context)
        {
            await _channel.Writer.WaitToWriteAsync();
            await _channel.Writer.WriteAsync(context.Message);
        }
    }
}
