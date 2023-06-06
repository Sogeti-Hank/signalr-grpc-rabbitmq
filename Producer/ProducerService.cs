using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Messages;

namespace Producer;

public class ProducerService : IHostedService
{
    private readonly IBus _bus;
    private readonly ILogger _logger;
    private Timer? _timer;

    private List<string> _statusCodes = new() { "Queued", "Running", "Completed", "Failed", "Stopped", "Processed", "Stalled" };

    public ProducerService(IBus bus, ILogger<ProducerService> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Service} is running.", nameof(ProducerService));
        _timer = new Timer(Produce, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

        return Task.CompletedTask;
    }

    private async void Produce(object? state)
    {
        _logger.LogInformation("Sending message");

        var message = new PipelineMessage("12345", _statusCodes[Random.Shared.Next(0, _statusCodes.Count - 1)]);

        await _bus.Send(message);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
