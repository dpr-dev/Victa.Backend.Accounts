using IdentityServer4.Configuration;
using IdentityServer4.Models;

using Microsoft.AspNetCore.Identity;

using MongoDB.Bson.Serialization;

using Victa.Backend.Accounts.Domain.Models.UserAggregate;
using Victa.Backend.Accounts.Infrastructure.Configuration.Identity.Adapters;
using Victa.Backend.Accounts.Infrastructure.Configuration.IdentityServer.Adapters.Services;
using Victa.Backend.Accounts.Infrastructure.Configuration.IdentityServer.Adapters.Stores.Configurational;
using Victa.Backend.Accounts.Infrastructure.Configuration.IdentityServer.Adapters.Stores.Operational;
using Victa.Backend.Accounts.Infrastructure.Configuration.IdentityServer.Adapters.Validation;
using Victa.Backend.Infrastructure.IdentityServer.Validation;

namespace Victa.Backend.Accounts.Infrastructure.Configuration.IdentityServer;

public static class IdentityServerWebBuilderExtensions
{
    public static WebApplicationBuilder ConfigureIdentityServer(this WebApplicationBuilder builder)
    {
        _ = builder.Services
            .AddIdentityServer()
            .AddClientStore<ClientStore>()
            .AddResourceStore<ResourceStore>()
            .AddPersistedGrantStore<PersistedGrantStore>()
            .AddDeviceFlowStore<DeviceFlowStore>()
            .AddProfileService<ProfileService>()
            .AddResourceOwnerValidator<ResourceOwnerValidator>()
            .AddExtensionGrantValidator<ExtensionGrantValidator>();

        _ = builder.Services.AddScoped<UserStore>();
        _ = builder.Services.AddScoped<IGrantHandler, GoogleGrantHandler>();
        _ = builder.Services.AddScoped<IGrantHandler, FacebookGrantHandler>();
        _ = builder.Services.AddScoped<IGrantHandler, AppleGrantHandler>();
        _ = builder.Services.AddScoped<IdentityErrorDescriber>();
        _ = builder.Services.AddScoped<IPasswordValidator<AccountsUser>, PasswordValidator<AccountsUser>>();
        _ = builder.Services.AddScoped<IPasswordHasher<AccountsUser>, PasswordHasher<AccountsUser>>();

        _ = builder.Services.AddOptions<IdentityServerOptions>()
            .Configure<IConfiguration>((options, cfg) => options.IssuerUri = cfg.GetValue("IS4_ISSUER_URI", "https://accounts.victa.ai"));


        _ = BsonClassMap.RegisterClassMap<Resource>(cfg =>
        {
            cfg.AutoMap();
            _ = cfg.MapIdField(x => x.Name);
            cfg.SetIsRootClass(true);
        });

        _ = BsonClassMap.RegisterClassMap<ApiScope>();
        _ = BsonClassMap.RegisterClassMap<ApiResource>();
        _ = BsonClassMap.RegisterClassMap<IdentityResource>();
        _ = BsonClassMap.RegisterClassMap<IdentityResources.Email>();
        _ = BsonClassMap.RegisterClassMap<IdentityResources.OpenId>();
        _ = BsonClassMap.RegisterClassMap<IdentityResources.Address>();
        _ = BsonClassMap.RegisterClassMap<IdentityResources.Profile>();
        _ = BsonClassMap.RegisterClassMap<IdentityResources.Phone>();
        _ = BsonClassMap.RegisterClassMap<Client>(cfg => cfg.MapIdField(x => x.ClientId).ClassMap.AutoMap());
        _ = BsonClassMap.RegisterClassMap<PersistedGrant>(cfg => cfg.MapIdField(x => x.Key).ClassMap.AutoMap());

        return builder;
    }
}
