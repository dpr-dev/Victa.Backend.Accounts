using Pulumi;

namespace Victa.Backend.Accounts.Deployment.Configs;

public class AppSettings
{
    public AppSettings(Config config)
    {
        DbName = config.Require("DB_NAME");
        DbConn = config.RequireSecret("DB_CONN");
    }


    public string DbName { get; private set; }
    public Output<string> DbConn { get; private set; }
}
