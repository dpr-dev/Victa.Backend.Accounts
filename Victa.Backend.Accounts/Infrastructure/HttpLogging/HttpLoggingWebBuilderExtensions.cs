namespace Victa.Backend.Accounts.Infrastructure.HttpLogging;

public static class HttpLoggingWebBuilderExtensions
{
    public static WebApplicationBuilder ConfigureHttpLogging(this WebApplicationBuilder builder)
    {
        _ = builder.Services.AddHttpLogging(cfg =>
        {
            _ = cfg.RequestHeaders.Add("x-forwarded-for");
            _ = cfg.RequestHeaders.Add("x-forwarded-proto");
            _ = cfg.RequestHeaders.Add("X-Cloud-Trace-Context");
        });

        return builder;
    }
}
