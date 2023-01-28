using System.Text.Json;
using System.Text.Json.Serialization;

namespace Victa.Backend.Accounts.Infrastructure.JsonOptions;

public static class JsonOptionsWebBuilderExtensions
{
    public static WebApplicationBuilder ConfigureJsonOptions(this WebApplicationBuilder builder)
    {
        _ = builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(cfg =>
        {
            cfg.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));
        });

        return builder;
    }
}
