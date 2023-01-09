using Pulumi;

using Victa.Backend.Accounts.Deployment.Configs;

using Gcp = Pulumi.Gcp;

namespace Victa.Backend.Accounts.Deployment;

public class StackSecrets
{
    public StackSecrets(DeploymentCfg cfg)
    {
        DbConn = CreateDefaultSecret($"ACCOUNTS-SERVICE-DB_URL-{cfg.Deployment.ResourcePrefix.ToUpper()}", cfg.App.DbConn);
    }

    public Gcp.SecretManager.Secret DbConn { get; }


    private static Gcp.SecretManager.Secret CreateDefaultSecret(string name, Input<string> data)
    {

        Gcp.SecretManager.Secret secret = new(name, new()
        {
            SecretId = name,
            Replication = new Gcp.SecretManager.Inputs.SecretReplicationArgs { Automatic = true, },
            Labels =
            {
                { "type", "service" },
                { "service", "backend" },
            }
        });

        _ = new Gcp.SecretManager.SecretVersion($"{name}-value", new()
        {
            Enabled = true,
            Secret = secret.Id,
            SecretData = data
        });

        return secret;
    }
}
