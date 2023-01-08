using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

using MongoDB.Bson.Serialization;
using MongoDB.Driver;

using Victa.Backend.Accounts.Infrastructure.Configuration.DataProtection.Adapters;

namespace Victa.Backend.Accounts.Infrastructure.Configuration.DataProtection;

public static class MongoDbDataProtectionBuilderExtensions
{
    public static IDataProtectionBuilder PersistKeysToMongoDb(this IDataProtectionBuilder builder)
    {
        _ = BsonClassMap.RegisterClassMap<XmlEntry>(cfg =>
        {
            cfg.MapIdField(x => x.Key).ClassMap.AutoMap();
        });

        _ = builder.Services
            .AddSingleton<MongoDbXmlRepository>()
            .AddOptions<KeyManagementOptions>()
                .Configure<MongoDbXmlRepository>((options, repository) => options.XmlRepository = repository);


        _ = builder.Services
            .AddSingleton(provider => provider
                .GetRequiredService<IMongoDatabase>().GetCollection<XmlEntry>("XmlEntries"));

        return builder;
    }
}
