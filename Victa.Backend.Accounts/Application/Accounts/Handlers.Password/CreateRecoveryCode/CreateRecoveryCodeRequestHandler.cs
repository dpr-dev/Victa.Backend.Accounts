using MediatR;

using Microsoft.AspNetCore.Identity;

using Victa.Backend.Accounts.Contracts.Events.Accounts;
using Victa.Backend.Accounts.Core;
using Victa.Backend.Accounts.Domain.Models.UserAggregate;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Password.CreateRecoveryCode;

public class CreateRecoveryCodeRequestHandler
    : IRequestHandler<CreateRecoveryCodeRequest, CreateRecoveryCodeResponse>
{
    private readonly ILogger _logger;
    private readonly IServiceBus _serviceBus;
    private readonly UserManager<AccountsUser> _userManager;

    public CreateRecoveryCodeRequestHandler(
        ILogger<CreateRecoveryCodeRequestHandler> logger,
        IServiceBus serviceBus,
        UserManager<AccountsUser> userManager)
    {
        _logger = logger;
        _serviceBus = serviceBus;
        _userManager = userManager;
    }

    public async Task<CreateRecoveryCodeResponse> Handle(CreateRecoveryCodeRequest request,
        CancellationToken cancellationToken)
    {
        AccountsUser? user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return CreateRecoveryCodeResponse.UserNotFoundByEmail(request.Email);
        }


        PasswordRecoveryToken recoveryToken =
            await user.GeneratePasswordRecoveryToken(
                _userManager.GeneratePasswordResetTokenAsync);

        try
        {
            await _serviceBus.Publish(new RecoveryCodeCreated
            {
                UserId = user.Id,
                RecoveryCode = recoveryToken.RecoveryCode
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unable to send event ({eventName}) for user (Id={user})",
                nameof(RecoveryCodeCreated), user.Id);
        }

        return CreateRecoveryCodeResponse.Success();
    }
}
