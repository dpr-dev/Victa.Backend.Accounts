using MediatR;

using MongoDB.Driver;

using Victa.Backend.Accounts.Domain.Models.UserAggregate;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Logout;

public sealed class LogoutRequestHandler
    : IRequestHandler<LogoutRequest, LogoutResponse>
{
    private readonly ILogger _logger;
    private readonly IMongoCollection<AccountsUser> _users;

    public LogoutRequestHandler(
        ILogger<LogoutRequestHandler> logger, 
        IMongoCollection<AccountsUser> users)
    {
        _logger = logger;
        _users = users;
    }

    public async Task<LogoutResponse> Handle(LogoutRequest request,
        CancellationToken cancellationToken)
    {
        List<UpdateDefinition<AccountsUser>> builders = new();
        FilterDefinition<AccountsUser> filter =
            Builders<AccountsUser>.Filter.Where(x => x.Id == request.UserId);

        if (!string.IsNullOrEmpty(request.FirebaseToken))
        {
            _logger.LogInformation(
                "Firebase token exists, add to builders");

            builders.Add(Builders<AccountsUser>.Update.PullFilter(x => x.FirebaseTokens, request.FirebaseToken));
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

            return LogoutResponse.Unhandled;
        }

        return LogoutResponse.Success;
    }
}
