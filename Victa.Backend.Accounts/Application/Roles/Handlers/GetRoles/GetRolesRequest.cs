using MediatR;

using Victa.Backend.Accounts.Contracts.Input.Roles;

namespace Victa.Backend.Accounts.Application.Roles.Handlers.GetRoles;


public sealed class GetRolesRequest : IRequest<GetRolesResponse>
{
    public string UserId { get; set; }
    public CreateRoleBody Body { get; set; }
}

