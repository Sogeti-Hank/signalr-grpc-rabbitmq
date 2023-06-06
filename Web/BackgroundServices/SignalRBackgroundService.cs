using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using GrpcService;
using Web.Hubs;

namespace Web.HostedServices;

public class SignalRBackgroundService : BackgroundService
{
    private readonly PipelineSubscriber.PipelineSubscriberClient _client;
    private readonly IPipelinePublisher _publisher;
    private readonly ILogger<SignalRBackgroundService> _logger;

    public SignalRBackgroundService(PipelineSubscriber.PipelineSubscriberClient client, IPipelinePublisher publisher, ILogger<SignalRBackgroundService> logger)
    {
        _client = client;
        _publisher = publisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // let other services start
        await Task.Yield();

        using var stream = _client.Subscribe(new Empty(), cancellationToken: cancellationToken);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await foreach (PipelineResponse message in stream.ResponseStream.ReadAllAsync(cancellationToken))
                {
                    _logger.LogInformation($"SignalR Background Service Publish: {message.ProjectId} {message.Status}");
                    await _publisher.Publish(message);
                }
            }

        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            Console.WriteLine("Stream cancelled.");
        }
    }
    
}