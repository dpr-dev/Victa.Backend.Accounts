using MediatR;

using MongoDB.Driver;

using Victa.Backend.Accounts.Domain.Models.UserAggregate;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Configure;

public sealed class ConfigureRequestHandler
    : IRequestHandler<ConfigureRequest, ConfigureResponse>
{
    private readonly ILogger _logger;
    private readonly IMongoCollection<AccountsUser> _users;

    public ConfigureRequestHandler(
        ILogger<ConfigureRequestHandler> logger,
        IMongoCollection<AccountsUser> users)
    {
        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        if (users is null)
        {
            throw new ArgumentNullException(nameof(users));
        }

        _logger = logger;
        _users = users;
    }

    public async Task<ConfigureResponse> Handle(ConfigureRequest request,
        CancellationToken cancellationToken)
    {
        List<UpdateDefinition<AccountsUser>> builders = new();
        FilterDefinition<AccountsUser> filter =
            Builders<AccountsUser>.Filter.Where(x => x.Id == request.UserId);

        if (!string.IsNullOrEmpty(request.FirebaseToken))
        {
            _logger.LogInformation(
                "Firebase token exists, add to builders");

            builders.Add(Builders<AccountsUser>.Update.AddToSet(x => x.FirebaseTokens, request.FirebaseToken));
        }

        try
        {
            _ = await _users.UpdateOneAsync(filter, Builders<AccountsUser>.Update.Combine(builders), cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unable to update user (UserId={user})",
                request.UserId);

            return ConfigureResponse.Unhandled;
        }

        return ConfigureResponse.Success;
    }
}
