using AutoMapper;

using MediatR;

using Microsoft.AspNetCore.Identity;

using Victa.Backend.Accounts.Contracts;
using Victa.Backend.Accounts.Contracts.Output.Accounts;
using Victa.Backend.Accounts.Domain.Models.UserAggregate;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Registration.RegisterViaPassword;

public class RegisterViaPasswordRequestHandler
    : IRequestHandler<RegisterViaPasswordRequest, RegisterViaPasswordResponse>
{
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly UserManager<AccountsUser> _userManager;

    public RegisterViaPasswordRequestHandler(
        IMapper mapper,
        ILogger<RegisterViaPasswordRequestHandler> logger,
        UserManager<AccountsUser> userManager)
    {
        if (mapper is null)
        {
            throw new ArgumentNullException(nameof(mapper));
        }

        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        if (userManager is null)
        {
            throw new ArgumentNullException(nameof(userManager));
        }

        _mapper = mapper;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<RegisterViaPasswordResponse> Handle(RegisterViaPasswordRequest request,
        CancellationToken cancellationToken)
    {
        AccountsUser user = new(request.Source.Email,
            request.Source.UserName, roles: new() { "customer" })
        {
            Age = request.Source.Age,
            InvitedBy = request.Source.InvitedBy,
            PromotedBy = request.Source.PromotedBy,
            Gender = request.Source.Gender switch
            {
                SGender.Male => Gender.Male,
                SGender.Female => Gender.Female,
                SGender.NoBinary => Gender.NoBinary,
                _ => Gender.NotToSay
            }
        };

        IdentityResult createResult = await _userManager.CreateAsync(user, request.Source.Password);
        if (!createResult.Succeeded)
        {
        }

        return RegisterViaPasswordResponse.Success(_mapper.Map<OAccountsUser>(user));
    }
}
