using Microsoft.Extensions.Configuration;

using PulumiConfig = Pulumi.Config;

namespace Victa.Backend.Accounts.Deployment.Configs;

public class DeploymentCfg
{
    public DeploymentCfg(IConfiguration configuration)
    {
        RawCiStage = configuration.GetValue<string?>("CI_STAGE")?.ToLower()
            ?? throw new InvalidOperationException("Unable to resolve GITHUB_REF_NAME env variable");

        BuildDirectory = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName
            ?? throw new InvalidOperationException("Unable to resolve parent directory path");

        if (string.Equals(RawCiStage, "dev"))
        {
            Stage = Stage.Dev;
        }
        else if (string.Equals(RawCiStage, "prod"))
        {
            Stage = Stage.Prod;
        }
        else
        {
            throw new InvalidOperationException($"Unknown CI_STAGE env variable (Env='{RawCiStage}')");
        }

        GithubRefName = configuration.GetValue<string>("GITHUB_REF_NAME")
            ?? throw new InvalidOperationException("Unable to resolve GITHUB_REF_NAME env variable");

        App = new AppSettings(new PulumiConfig("app"));
        Gcp = new GcpSettings(new PulumiConfig("gcp"));
        Deployment = new DeploymentSettings(new PulumiConfig("deployment"));
        Container = new ContainerSettings(new PulumiConfig("container"));
    }

    public string BuildDirectory { get; }
    public string GithubRefName { get; }

    public Stage Stage { get; }
    public string RawCiStage { get; }

    public AppSettings App { get; }
    public GcpSettings Gcp { get; }
    public DeploymentSettings Deployment { get; }
    public ContainerSettings Container { get; }

    public bool IsProd()
    {
        return Stage == Stage.Prod;
    }
}
