using System.Reflection;

using MediatR;

namespace Victa.Backend.Accounts.Infrastructure.Configuration.MediatR;

public static class MediatRWebBuilderExtensions
{
    public static WebApplicationBuilder ConfigureMediatR(this WebApplicationBuilder builder)
    {
        _ = builder.Services.AddMediatR(GetAssemblies());

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
