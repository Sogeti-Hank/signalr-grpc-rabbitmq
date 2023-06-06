using Producer;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Shared.Messages;
using Microsoft.Extensions.Logging;

// Fake Orleans Silo

HostApplicationBuilder builder = Host.CreateApplicationBuilder();

builder.Services.AddLogging(configure => configure.AddConsole());

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((_, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // will send to exchange only
        EndpointConvention.Map<PipelineMessage>(new Uri("exchange:pipeline-subscribers"));

        cfg.Send<PipelineMessage>(x =>
        {
            // use ProjectId for the routing key
            x.UseRoutingKeyFormatter(context => context.Message.ProjectId);
        });

    });
});

builder.Services.AddHostedService<ProducerService>();

var host = builder.Build();
host.Run();
