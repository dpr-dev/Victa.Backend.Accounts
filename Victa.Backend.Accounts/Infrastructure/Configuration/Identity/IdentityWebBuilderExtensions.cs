using Microsoft.AspNetCore.Identity;

using Victa.Backend.Accounts.Domain.Models.RoleAggregate;
using Victa.Backend.Accounts.Domain.Models.UserAggregate;
using Victa.Backend.Accounts.Infrastructure.Configuration.Identity.Adapters;

namespace Victa.Backend.Accounts.Infrastructure.Configuration.Identity;

public static class IdentityWebBuilderExtensions
{
    public static WebApplicationBuilder ConfigureIdentity(this WebApplicationBuilder builder)
    {
        _ = builder.Services
           .AddIdentity<AccountsUser, AccountsRole>()
           .AddDefaultTokenProviders()
           .AddUserManager<UserManager<AccountsUser>>()
           .AddRoleManager<RoleManager<AccountsRole>>()
           .AddRoleStore<RoleStore>()
           .AddUserStore<UserStore>();

        _ = builder.Services.AddTransient<IRoleStore<AccountsRole>, RoleStore>();
        _ = builder.Services.AddTransient<IUserStore<AccountsUser>, UserStore>();

        _ = builder.Services.AddOptions<IdentityOptions>()
            .Configure<IConfiguration>((options, cfg) => cfg.GetRequiredSection("Identity").Bind(options));

        return builder;
    }
}
