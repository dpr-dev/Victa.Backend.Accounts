using Google.Apis.Auth.OAuth2;
using Google.Cloud.Diagnostics.AspNetCore3;

using IdentityServer4.Models;

using MongoDB.Driver;

using Victa.Backend.Accounts.Infrastructure.Configuration.AutoMapper;
using Victa.Backend.Accounts.Infrastructure.Configuration.Cors;
using Victa.Backend.Accounts.Infrastructure.Configuration.FluentValidation;
using Victa.Backend.Accounts.Infrastructure.Configuration.Identity;
using Victa.Backend.Accounts.Infrastructure.Configuration.IdentityServer;
using Victa.Backend.Accounts.Infrastructure.Configuration.JsonOptions;
using Victa.Backend.Accounts.Infrastructure.Configuration.MediatR;
using Victa.Backend.Accounts.Infrastructure.Configuration.Mongo;
using Victa.Backend.Accounts.Infrastructure.Configuration.UrlRewriter;

WebApplicationBuilder builder =
    WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddHttpLogging(cfg =>
{
    _ = cfg.RequestHeaders.Add("x-forwarded-for");
    _ = cfg.RequestHeaders.Add("x-forwarded-proto");
    _ = cfg.RequestHeaders.Add("X-Cloud-Trace-Context");
});

builder.ConfigureIdentity();
builder.ConfigureIdentityServer();
builder.ConfigureUrlRewriter();
builder.ConfigureCors();
builder.ConfigureJsonOptions();
builder.ConfigureMongo();
builder.ConfigureAutoMapper();
builder.ConfigureFluentValidation();
builder.ConfigureMediatR();

builder.Services.AddSingleton(
    new Lazy<GoogleCredential>(GoogleCredential.GetApplicationDefault));

if (builder.Environment.IsProduction())
{
    _ = builder.Services.AddGoogleDiagnosticsForAspNetCore();
}

// mongodb 

builder.Services.AddSingleton(provider => provider.GetRequiredService<IMongoDatabase>().GetCollection<Client>("Clients"));
builder.Services.AddSingleton(provider => provider.GetRequiredService<IMongoDatabase>().GetCollection<Resource>("Resources"));
builder.Services.AddSingleton(provider => provider.GetRequiredService<IMongoDatabase>().GetCollection<PersistedGrant>("PersistedGrants"));


WebApplication webapp = builder.Build();

webapp.UseHttpLogging();
webapp.UseHttpsRedirection();
webapp.UseCors();
webapp.UseRewriter();
webapp.UseStaticFiles();
webapp.UseAuthorization();
webapp.UseIdentityServer();
webapp.MapControllers();

await webapp.RunAsync();
