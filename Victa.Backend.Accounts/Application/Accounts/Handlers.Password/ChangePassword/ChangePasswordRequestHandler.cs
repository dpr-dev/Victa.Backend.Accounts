using MediatR;

using Microsoft.AspNetCore.Identity;

using Victa.Backend.Accounts.Domain.Models.UserAggregate;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Password.ChangePassword;

public sealed class ChangePasswordRequestHandler
    : IRequestHandler<ChangePasswordRequest, ChangePasswordResponse>
{
    private readonly ILogger _logger;
    private readonly UserManager<AccountsUser> _userManager;

    public ChangePasswordRequestHandler(
        ILogger<ChangePasswordRequestHandler> logger,
        UserManager<AccountsUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<ChangePasswordResponse> Handle(ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        AccountsUser? user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return ChangePasswordResponse.UserNotFoundById(request.UserId);
        }

        IdentityResult result = await user.ChangePassword(CreatePasswordHash, ValidatePassword, request.Password);
        if (!result.Succeeded)
        {
            // TODO: improve returned error
            return ChangePasswordResponse.Unhandled;
        }

        try
        {
            _ = await _userManager.UpdateAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unable to update user (Id={id})",
                user.Id);

            return ChangePasswordResponse.Unhandled;
        }

        return ChangePasswordResponse.Success();


        string CreatePasswordHash(string password)
        {
            return _userManager.PasswordHasher.HashPassword(user, password);
        }

        async Task<IdentityResult> ValidatePassword(string password)
        {
            foreach (IPasswordValidator<AccountsUser> validator in _userManager.PasswordValidators)
            {
                IdentityResult result = await validator.ValidateAsync(_userManager, user, password);
                if (result.Succeeded == false)
                {
                    return result;
                }
            }

            return IdentityResult.Success;
        }
    }
}
