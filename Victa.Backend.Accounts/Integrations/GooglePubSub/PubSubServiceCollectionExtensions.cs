using Victa.Backend.Accounts.Core;

namespace Victa.Backend.Accounts.Integrations.GooglePubSub;

public static class PubSubServiceCollectionExtensions
{
    public static WebApplicationBuilder AddGoogleCloudPubSub(this WebApplicationBuilder builder)
    {
        _ = builder.Services.AddSingleton<IServiceBus, PubSubServiceBus>()
            .AddOptions<PubSubOptions>();


        return builder;
    }
}
