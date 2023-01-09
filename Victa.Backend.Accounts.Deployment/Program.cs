using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Victa.Backend.Accounts.Deployment.Configs;

IHostBuilder host = Host
    .CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        _ = services.AddSingleton<VictaStack>();
        _ = services.AddHostedService<DeploymentRunnerHostedService>();
        _ = services.AddSingleton<DeploymentCfg>();
    });

await host.StartAsync();
