using Pulumi;

namespace Victa.Backend.Accounts.Deployment.Configs;

public class GcpSettings
{
    public GcpSettings(Config config)
    {
        Project = config.Require("project");
    }

    public string Project { get; private set; }
}
