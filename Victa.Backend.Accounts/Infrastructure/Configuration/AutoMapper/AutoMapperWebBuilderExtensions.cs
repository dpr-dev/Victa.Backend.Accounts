using System.Reflection;

namespace Victa.Backend.Accounts.Infrastructure.Configuration.AutoMapper;

public static class AutoMapperWebBuilderExtensions
{
    public static WebApplicationBuilder ConfigureAutoMapper(this WebApplicationBuilder builder)
    {
        _ = builder.Services.AddAutoMapper(GetAssemblies());

        return builder;
    }

    private static Assembly[] GetAssemblies()
    {
        return new[]
        {
            Assembly.Load("Victa.Backend.Accounts"),
        };
    }
}
