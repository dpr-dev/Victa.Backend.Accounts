namespace Victa.Backend.Accounts.Infrastructure.Cors;

public static class CorsWebBuilderExtensions
{
    public static WebApplicationBuilder ConfigureCors(this WebApplicationBuilder builder)
    {
        _ = builder.Services.AddCors(cfg =>
        {
            //cfg.AddDefaultPolicy(x => x.AllowCredentials().AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin());
        });

        return builder;
    }
}
