using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Victa.Backend.Accounts.Infrastructure.DataProtection.Adapters;

namespace Victa.Backend.Accounts.Infrastructure.DataProtection;

public static class DataProtectionWebBuilderExtensions
{
    public static WebApplicationBuilder ConfigureDataProtection(this WebApplicationBuilder builder)
    {
        _ = BsonClassMap
            .RegisterClassMap<XmlEntry>(cfg => cfg.AutoMap())
            .MapIdProperty(x => x.Key);

        _ = builder.Services
            .AddSingleton<MongoDbXmlRepository>()
            .AddOptions<KeyManagementOptions>()
                .Configure<MongoDbXmlRepository>((options, repository) => options.XmlRepository = repository);

        _ = builder.Services
            .AddSingleton(provider => provider
                .GetRequiredService<IMongoDatabase>().GetCollection<XmlEntry>("XmlEntries"));

        _ = builder.Services.AddDataProtection()
            .SetApplicationName(builder.Configuration.GetValue("APP_NAME", "Victa")!);

        return builder;
    }
}
