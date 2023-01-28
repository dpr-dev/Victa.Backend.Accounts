using System.Reflection;

using FluentValidation;
using FluentValidation.AspNetCore;

namespace Victa.Backend.Accounts.Infrastructure.FluentValidation;

public static class FluentValidationWebBuilderExtensions
{
    public static WebApplicationBuilder ConfigureFluentValidation(this WebApplicationBuilder builder)
    {
        Assembly[] assemblies = new[]
        {
            Assembly.Load("Victa.Backend.Accounts"),
        };

        _ = builder.Services
            .AddValidatorsFromAssemblies(assemblies)
            .AddFluentValidationAutoValidation(cfg =>
            {
                cfg.DisableDataAnnotationsValidation = true;
            });

        return builder;
    }
}
