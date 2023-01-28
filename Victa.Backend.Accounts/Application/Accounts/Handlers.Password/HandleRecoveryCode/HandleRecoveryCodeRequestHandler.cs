using MediatR;

using Microsoft.AspNetCore.Identity;

using Victa.Backend.Accounts.Domain.Models.UserAggregate;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Password.HandleRecoveryCode;

public class HandleRecoveryCodeRequestHandler
    : IRequestHandler<HandleRecoveryCodeRequest, HandleRecoveryCodeResponse>
{
    private readonly ILogger _logger;
    private readonly UserManager<AccountsUser> _userManager;

    public HandleRecoveryCodeRequestHandler(
        ILogger<HandleRecoveryCodeRequestHandler> logger,
        UserManager<AccountsUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<HandleRecoveryCodeResponse> Handle(HandleRecoveryCodeRequest request,
        CancellationToken cancellationToken)
    {
        AccountsUser? user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return HandleRecoveryCodeResponse.UserNotFoundByEmail(request.Email);
        }

        IdentityResult result = await _userManager.ResetPasswordAsync(user, request.RecoveryCode, request.Password);
        if (result is
            {
                Succeeded: false
            })
        {
            return HandleRecoveryCodeResponse.Unhandled;
        }


        return HandleRecoveryCodeResponse.Success();
    }
}
