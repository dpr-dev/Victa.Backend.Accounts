using AutoMapper;

using MediatR;

using Microsoft.AspNetCore.Identity;

using Victa.Backend.Accounts.Contracts;
using Victa.Backend.Accounts.Contracts.Events.Accounts;
using Victa.Backend.Accounts.Contracts.Output.Accounts;
using Victa.Backend.Accounts.Core;
using Victa.Backend.Accounts.Domain.Models.UserAggregate;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Registration.RegisterViaPassword;

public class RegisterViaPasswordRequestHandler
    : IRequestHandler<RegisterViaPasswordRequest, RegisterViaPasswordResponse>
{
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly UserManager<AccountsUser> _userManager;
    private readonly IServiceBus _bus;

    public RegisterViaPasswordRequestHandler(
        IMapper mapper,
        ILogger<RegisterViaPasswordRequestHandler> logger,
        UserManager<AccountsUser> userManager,
        IServiceBus bus)
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

        if (bus is null)
        {
            throw new ArgumentNullException(nameof(bus));
        }

        _mapper = mapper;
        _logger = logger;
        _userManager = userManager;
        _bus = bus;
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
            AvatarId = request.Source.AvatarId,
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
            return RegisterViaPasswordResponse.Unhandled;
        }

        OAccountsUser output =
            _mapper.Map<OAccountsUser>(user);

        try
        {
            await _bus.Publish(new AccountsUserCreated { AccountsUser = output });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unable to send event about user creation");
        }

        return RegisterViaPasswordResponse.Success(output);
    }
}
