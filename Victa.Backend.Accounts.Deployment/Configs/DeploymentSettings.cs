using Pulumi;

namespace Victa.Backend.Accounts.Deployment.Configs;

public class DeploymentSettings
{
    public DeploymentSettings(Config config)
    {
        ResourcePrefix = config.Require("resourcePrefix");
    }

    public string ResourcePrefix { get; private set; }
}
