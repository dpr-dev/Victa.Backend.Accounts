using Google.Apis.Auth.OAuth2;
using Google.Cloud.Diagnostics.AspNetCore3;

using Victa.Backend.Accounts.Core;
using Victa.Backend.Accounts.Infrastructure.Integrations.Google.PubSub;

namespace Victa.Backend.Accounts.Infrastructure.Integrations.Google;

public static class GoogleCloudWebBuilderExtensions
{
    public static WebApplicationBuilder ConfigureGoogleCloudIntegration(this WebApplicationBuilder builder)
    {
        _ = builder.Services.AddSingleton(GoogleCredential.GetApplicationDefault());
        _ = builder.Services.AddSingleton<IServiceBus, PubSubServiceBus>()
            .AddOptions<PubSubOptions>()
                .Configure<IConfiguration>((options, cfg) =>
                {
                    options.ProjectId = cfg.GetValue<string>("GCP_PROJECT_ID");
                    options.ResourcePrefix = cfg.GetValue<string>("RESOURCE_PREFIX");
                });


        if (IsProduction(builder))
        {
            ConfigureProductionDiagnistics(builder);
        }
        else
        {
            ConfigureDevelopmentDiagnostics(builder);
        }

        return builder;
    }

    private static bool IsProduction(WebApplicationBuilder builder)
    {
        return builder.Environment.IsProduction();
    }

    private static void ConfigureProductionDiagnistics(WebApplicationBuilder builder)
    {
        _ = builder.Logging.ClearProviders();
        _ = builder.Services.AddGoogleDiagnosticsForAspNetCore();
    }

    private static void ConfigureDevelopmentDiagnostics(WebApplicationBuilder builder)
    {
        string? project =
                        builder.Configuration.GetValue<string>("GCLOUD_PROJECT")
                        ?? builder.Configuration.GetValue<string>("GOOGLE_CLOUD_PROJECT")
                        ?? builder.Configuration.GetValue<string>("GCP_PROJECT_ID");

        _ = builder.Services.AddGoogleTraceForAspNetCore(new()
        {
            ServiceOptions = new() { ProjectId = project }
        });

        _ = builder.Services.AddGoogleErrorReportingForAspNetCore(new()
        {
            ProjectId = project
        });
    }
}
