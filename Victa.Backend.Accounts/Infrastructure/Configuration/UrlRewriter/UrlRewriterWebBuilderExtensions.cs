using Microsoft.AspNetCore.Rewrite;

namespace Victa.Backend.Accounts.Infrastructure.Configuration.UrlRewriter;

public static class UrlRewriterWebBuilderExtensions
{
    public static WebApplicationBuilder ConfigureUrlRewriter(this WebApplicationBuilder builder)
    {
        _ = builder.Services.AddOptions<RewriteOptions>()
            .Configure(rewriteOptions => rewriteOptions
                .AddRewrite("(.*)/$", "$1", skipRemainingRules: false));

        return builder;
    }
}
