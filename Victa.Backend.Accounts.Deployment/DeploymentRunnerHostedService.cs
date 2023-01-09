using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Victa.Backend.Accounts.Deployment;

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
        _ = await Pulumi.Deployment.RunAsync<AccountsStack>(scope.ServiceProvider);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }
}
