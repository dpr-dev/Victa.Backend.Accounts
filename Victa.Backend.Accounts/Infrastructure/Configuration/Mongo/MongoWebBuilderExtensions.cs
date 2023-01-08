using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

using Victa.Backend.Accounts.Domain.Models.RoleAggregate;
using Victa.Backend.Accounts.Domain.Models.UserAggregate;

namespace Victa.Backend.Accounts.Infrastructure.Configuration.Mongo;

public static class MongoWebBuilderExtensions
{
    public static WebApplicationBuilder ConfigureMongo(this WebApplicationBuilder builder)
    {
        ConventionRegistry.Register("camelCase",
            new ConventionPack { new CamelCaseElementNameConvention() }, t => true);

        _ = builder.Services.AddSingleton(provider => provider.GetRequiredService<IMongoClient>().GetDatabase("DB_NAME"));
        _ = builder.Services.AddSingleton<IMongoClient>(provider => new MongoClient(Environment.GetEnvironmentVariable("DB_CONN")));


        RegisterCollection<AccountsUser>(builder.Services, "Users", cfg =>
        {
            _ = cfg.MapIdProperty(x => x.Id);
            cfg.AutoMap();
        });

        RegisterCollection<AccountsRole>(builder.Services, "Roles", cfg =>
        {
            _ = cfg.MapIdProperty(x => x.Id);
            cfg.AutoMap();
        });

        return builder;
    }

    private static void RegisterCollection<T>(IServiceCollection services, string name, Action<BsonClassMap<T>> cfg)
    {

        _ = BsonClassMap.RegisterClassMap<T>(classMap =>
        {
            cfg(classMap);
        });

        _ = services.AddSingleton(provider => provider.GetRequiredService<IMongoDatabase>().GetCollection<T>(name));
    }
}
