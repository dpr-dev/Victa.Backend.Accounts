using AutoMapper;

using MediatR;

using MongoDB.Driver;

using Victa.Backend.Accounts.Contracts.Output.Accounts;
using Victa.Backend.Accounts.Domain.Models.UserAggregate;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.GetMe;

public sealed class GetMeRequestHandler
    : IRequestHandler<GetMeRequest, GetMeResponse>
{
    private readonly ILogger _logger;
    private readonly IMongoCollection<AccountsUser> _users;
    private readonly IMapper _mapper;

    public GetMeRequestHandler(
        ILogger<GetMeRequestHandler> logger,
        IMongoCollection<AccountsUser> users,
        IMapper mapper)
    {
        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        if (users is null)
        {
            throw new ArgumentNullException(nameof(users));
        }

        if (mapper is null)
        {
            throw new ArgumentNullException(nameof(mapper));
        }

        _logger = logger;
        _users = users;
        _mapper = mapper;
    }

    public async Task<GetMeResponse> Handle(GetMeRequest request,
        CancellationToken cancellationToken)
    {
        AccountsUser user;
        try
        {
            user = await _users.Find(x => x.Id == request.Id).FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }
        catch (Exception)
        {
            return GetMeResponse.Unhandled;
        }

        return GetMeResponse.Success(_mapper.Map<OAccountsUser>(user));
    }
}
