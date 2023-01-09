using MediatR;

namespace Victa.Backend.Accounts.Application.Roles.Handlers.GetRoles;

public sealed class GetRolesRequestHandler
    : IRequestHandler<GetRolesRequest, GetRolesResponse>
{
    public Task<GetRolesResponse> Handle(GetRolesRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

