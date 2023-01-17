using Google.Apis.Auth.OAuth2;
using Google.Cloud.Diagnostics.AspNetCore3;

using Hellang.Middleware.ProblemDetails;

using Microsoft.IdentityModel.Logging;

using Victa.Backend.Accounts.Core.AspNetCore.Authorization;
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
using Victa.Backend.Accounts.Integrations.GooglePubSub;

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

builder.AddGoogleCloudPubSub();

builder.Services.AddSingleton(new Lazy<GoogleCredential>(GoogleCredential.GetApplicationDefault));
builder.Services.AddAuthentication()
    .AddLocalApi();

builder.Services.AddAuthorization(x => x.AddDefaultPolicies());
builder.Services.AddProblemDetails(cfg =>
{
    cfg.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);
    cfg.IncludeExceptionDetails = (_, ex) =>
    {
        return builder.Environment.IsDevelopment();
    };
});


if (builder.Environment.IsProduction())
{
    _ = builder.Logging.ClearProviders();
    _ = builder.Services.AddGoogleDiagnosticsForAspNetCore();
}

WebApplication webapp = builder.Build();

webapp.UseHttpLogging(); 
webapp.UseProblemDetails();
webapp.UseCors();
webapp.UseRewriter();
webapp.UseStaticFiles();
webapp.UseAuthorization();
webapp.UseIdentityServer();
webapp.MapControllers();
webapp.MapRazorPages();

await webapp.RunAsync();
