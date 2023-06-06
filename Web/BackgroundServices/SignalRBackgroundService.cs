using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using GrpcService;
using Web.Hubs;

namespace Web.HostedServices;

public class SignalRBackgroundService : BackgroundService
{
    private readonly IPipelinePublisher _publisher;
    private readonly ILogger<SignalRBackgroundService> _logger;

    public SignalRBackgroundService(IPipelinePublisher publisher, ILogger<SignalRBackgroundService> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // let other services start
        await Task.Yield();

        GrpcChannel channel = CreateChannel();

        var client = new PipelineSubscriber.PipelineSubscriberClient(channel);

        using var stream = client.Subscribe(new Empty(), cancellationToken: cancellationToken);

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

    private static GrpcChannel CreateChannel()
    {
        var methodConfig = new MethodConfig
        {
            Names = { MethodName.Default },
            RetryPolicy = new RetryPolicy
            {
                MaxAttempts = 5,
                InitialBackoff = TimeSpan.FromSeconds(1),
                MaxBackoff = TimeSpan.FromSeconds(5),
                BackoffMultiplier = 1.5,
                RetryableStatusCodes = { StatusCode.Unavailable }
            }
        };

        var serviceConfig = new ServiceConfig
        {
            MethodConfigs = { methodConfig },
        };

        var channel = GrpcChannel.ForAddress("https://localhost:7284", new GrpcChannelOptions
        {

            ServiceConfig = serviceConfig,
            HttpHandler = new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true,
            },

        });

        return channel;
    }
}