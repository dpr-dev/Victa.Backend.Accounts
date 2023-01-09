using MediatR;

using Microsoft.AspNetCore.Mvc;

using Victa.Backend.Accounts.Application.Accounts.Handlers.Registration.RegisterViaPassword;
using Victa.Backend.Accounts.Contracts.Input.Accounts;
using Victa.Backend.Accounts.Core.AspNetCore.Mvc;

namespace Victa.Backend.Accounts.Controllers.Accounts;

[ApiController]
[Route("api/v1/accounts")]
public sealed class AccountsController : ApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IMediator mediator, ILogger<AccountsController> logger)
    {
        if (mediator is null)
        {
            throw new ArgumentNullException(nameof(mediator));
        }

        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        _mediator = mediator;
        _logger = logger;
    }


    #region Registration
    [HttpPost("registration/password")]
    public async Task<IActionResult> Register([FromBody] PasswordRegistrationBody source)
    {
        RegisterViaPasswordResponse result;
        try
        {
            result = await _mediator.Send(new RegisterViaPasswordRequest(source));
        }
        catch (Exception)
        {
            throw;
        }

        return result.Match(Json, Error);
    }
    #endregion
}
