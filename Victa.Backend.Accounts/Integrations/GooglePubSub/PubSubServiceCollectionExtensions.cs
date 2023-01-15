using Victa.Backend.Accounts.Core;

namespace Victa.Backend.Accounts.Integrations.GooglePubSub;

public static class PubSubServiceCollectionExtensions
{
    public static WebApplicationBuilder AddGoogleCloudPubSub(this WebApplicationBuilder builder)
    {
        _ = builder.Services.AddSingleton<IServiceBus, PubSubServiceBus>()
            .AddOptions<PubSubOptions>()
                .Configure<IConfiguration>((options, cfg) =>
                {
                    options.ProjectId = cfg.GetValue<string>("GCP_PROJECT_ID");
                    options.ResourcePrefix = cfg.GetValue<string>("RESOURCE_PREFIX");
                });


        return builder;
    }
}
