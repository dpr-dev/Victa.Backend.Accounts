using MediatR;

using Microsoft.AspNetCore.Identity;

using Victa.Backend.Accounts.Domain.Models.UserAggregate;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Password.VerifyPassword;

public class VerifyPasswordRequestHandler
    : IRequestHandler<VerifyPasswordRequest, VerifyPasswordResponse>
{
    private readonly ILogger _logger;
    private readonly UserManager<AccountsUser> _userManager;

    public VerifyPasswordRequestHandler(
        ILogger<VerifyPasswordRequestHandler> logger,
        UserManager<AccountsUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }


    public async Task<VerifyPasswordResponse> Handle(VerifyPasswordRequest request,
        CancellationToken cancellationToken)
    {
        AccountsUser? user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return VerifyPasswordResponse.UserNotFoundById(request.UserId);
        }

        bool result = await _userManager.CheckPasswordAsync(user, request.Password);
        if (result)
        {
            return VerifyPasswordResponse.Success();
        }

        return VerifyPasswordResponse.Unmatched;
    }
}
