using GrpcService.Consumers;
using GrpcService.Services;
using MassTransit;
using Shared.Messages;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(host =>
{
    host.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Each service instance is a competing consumer of the pipeline messages
        // SignalR backplane is used to distribute the messages to all clients
        cfg.ReceiveEndpoint("pipeline-subscribers", e =>
        {
            e.PrefetchCount = 1000;
            e.ConfigureConsumeTopology = false;
            e.Consumer<PipelineConsumer>(context);
        });
    });
});

// Add services to the container.
builder.Services.AddGrpc();

// Add dependencies
var channelOptions = new UnboundedChannelOptions();
var channel = Channel.CreateUnbounded<PipelineMessage>();
builder.Services.AddSingleton(channel);
builder.Services.AddScoped<PipelineConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<PipelineSubscriberService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
