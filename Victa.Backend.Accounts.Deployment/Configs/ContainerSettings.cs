using Pulumi;

namespace Victa.Backend.Accounts.Deployment.Configs;

public class ContainerSettings
{
    public ContainerSettings(Config config)
    {
        Cpu = config.Require("cpu");
        Memory = config.Require("memory");
        ExecutionEnvironment = config.Require("executionEnvironment");

        MaxScale = config.Require("maxScale");
        MinScale = config.Require("minScale");

        Port = config.RequireInt32("port");
        Concurrency = config.RequireInt32("containerConcurrency");
        StartupCpuBust = config.Require("startupCpuBust");

        Domain = config.Require("domain");
    }

    public string Cpu { get; private set; }
    public string Memory { get; private set; }
    public string MinScale { get; private set; }
    public string MaxScale { get; private set; }
    public string Domain { get; private set; }
    public int Port { get; private set; }
    public int Concurrency { get; private set; }
    public string ExecutionEnvironment { get; private set; }
    public string StartupCpuBust { get; private set; }
}
