using Google.Apis.Auth.OAuth2;
using Google.Cloud.Diagnostics.AspNetCore3;

using Microsoft.IdentityModel.Logging;

using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Victa.Backend.Accounts.Infrastructure.Configuration.AutoMapper;
using Victa.Backend.Accounts.Infrastructure.Configuration.Cors;
using Victa.Backend.Accounts.Infrastructure.Configuration.DataProtection;
using Victa.Backend.Accounts.Infrastructure.Configuration.FluentValidation;
using Victa.Backend.Accounts.Infrastructure.Configuration.Identity;
using Victa.Backend.Accounts.Infrastructure.Configuration.IdentityServer;
using Victa.Backend.Accounts.Infrastructure.Configuration.JsonOptions;
using Victa.Backend.Accounts.Infrastructure.Configuration.MediatR;
using Victa.Backend.Accounts.Infrastructure.Configuration.Mongo;
using Victa.Backend.Accounts.Infrastructure.Configuration.UrlRewriter;

WebApplicationBuilder builder =
    WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
}

builder.Services.AddMemoryCache();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpLogging(cfg =>
{
    _ = cfg.RequestHeaders.Add("x-forwarded-for");
    _ = cfg.RequestHeaders.Add("x-forwarded-proto");
    _ = cfg.RequestHeaders.Add("X-Cloud-Trace-Context");
});

builder.ConfigureMongo();
builder.ConfigureIdentity();
builder.ConfigureIdentityServer();
builder.ConfigureUrlRewriter();
builder.ConfigureCors();
builder.ConfigureJsonOptions();
builder.ConfigureAutoMapper();
builder.ConfigureFluentValidation();
builder.ConfigureMediatR();
builder.ConfigureDataProtection();

builder.Services.AddSingleton(new Lazy<GoogleCredential>(GoogleCredential.GetApplicationDefault));
builder.Services.AddAuthentication()
    .AddLocalApi();

if (builder.Environment.IsProduction())
{
    _ = builder.Services.AddGoogleDiagnosticsForAspNetCore();
}
else
{
    _ = builder.Services
        .AddOpenTelemetryTracing(b => b.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .SetResourceBuilder(ResourceBuilder
                .CreateDefault()
                .AddService("Victa.Accounts"))
            .AddJaegerExporter(opts =>
            {
                opts.AgentHost = builder.Configuration["JAEGER_AGENT_HOST"];
                opts.AgentPort = Convert.ToInt32(builder.Configuration["JAEGER_AGENT_PORT"]);
                opts.ExportProcessorType = ExportProcessorType.Batch;
            }));
}

WebApplication webapp = builder.Build();

webapp.UseHttpLogging();
webapp.UseHttpsRedirection();
webapp.UseCors();
webapp.UseRewriter();
webapp.UseStaticFiles();
webapp.UseAuthorization();
webapp.UseIdentityServer();
webapp.MapControllers();
webapp.MapRazorPages();

await webapp.RunAsync();
