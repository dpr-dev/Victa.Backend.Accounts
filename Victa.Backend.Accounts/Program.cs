using Hellang.Middleware.ProblemDetails;

using Microsoft.IdentityModel.Logging;

using Victa.Backend.Accounts.Core.AspNetCore.Authorization;
using Victa.Backend.Accounts.Infrastructure.AutoMapper;
using Victa.Backend.Accounts.Infrastructure.Cors;
using Victa.Backend.Accounts.Infrastructure.DataProtection;
using Victa.Backend.Accounts.Infrastructure.FluentValidation;
using Victa.Backend.Accounts.Infrastructure.HttpLogging;
using Victa.Backend.Accounts.Infrastructure.Identity;
using Victa.Backend.Accounts.Infrastructure.IdentityServer;
using Victa.Backend.Accounts.Infrastructure.Integrations.Google;
using Victa.Backend.Accounts.Infrastructure.JsonOptions;
using Victa.Backend.Accounts.Infrastructure.MediatR;
using Victa.Backend.Accounts.Infrastructure.Mongo;
using Victa.Backend.Accounts.Infrastructure.UrlRewriter;

WebApplicationBuilder builder =
    WebApplication.CreateBuilder(args);

IdentityModelEventSource.ShowPII =
    builder.Environment.IsProduction() != true;

builder.Services.AddMemoryCache();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

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
builder.ConfigureGoogleCloudIntegration();
builder.ConfigureHttpLogging();

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
