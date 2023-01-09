using System.Net.Sockets;
using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class DeploymentRunnerHostedService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    public DeploymentRunnerHostedService(
        ILogger<DeploymentRunnerHostedService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        _ = await Pulumi.Deployment.RunAsync<VictaStack>(scope.ServiceProvider);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }
}
