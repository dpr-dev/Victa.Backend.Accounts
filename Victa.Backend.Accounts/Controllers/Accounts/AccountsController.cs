using System.ComponentModel.DataAnnotations;
using System.Net;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Victa.Backend.Accounts.Application.Accounts.Handlers.GetMe;
using Victa.Backend.Accounts.Application.Accounts.Handlers.Registration.RegisterViaPassword;
using Victa.Backend.Accounts.Contracts.Input.Accounts;
using Victa.Backend.Accounts.Contracts.Input.Accounts.Validation;
using Victa.Backend.Accounts.Core.AspNetCore.Authorization;
using Victa.Backend.Accounts.Core.AspNetCore.Mvc;
using Victa.Backend.Accounts.Domain.Models.UserAggregate;

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

    [HttpGet]
    [AuthorizeSimple]
    public async Task<IActionResult> Me()
    {
        GetMeResponse result;
        try
        {
            result = await _mediator.Send(new GetMeRequest(UserId));
        }
        catch (Exception)
        {
            return Problem(statusCode: HttpStatusCode.InternalServerError);
        }

        return result.Match(Json, Error);
    }

    [HttpDelete]
    [ProducesResponseType(204)]
    [AuthorizeCustomer]
    public Task<IActionResult> Delete()
    {
        throw new NotImplementedException();
    }

    [HttpPost("configure")]
    [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
    [AuthorizeCustomer]
    public Task<IActionResult> Configure()
    {
        throw new NotImplementedException();
    }

    [HttpPost("logout")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
    [AuthorizeCustomer]
    public Task<IActionResult> Logout()
    {
        throw new NotImplementedException();
    }


    #region Registration
    [AllowAnonymous]
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



    #region Validation
    [HttpPost("validation/email")]
    public async Task<IActionResult> ValidateEmail([FromBody] ValidateEmailRequestBody request,
        [FromServices] UserManager<AccountsUser> userManager,
        [FromServices] IdentityErrorDescriber errorDescriber)
    {
        string? email = request.Value;
        if (string.IsNullOrEmpty(email))
        {
            ModelState.AddModelError(nameof(email), errorDescriber.InvalidEmail(email).Code);
        }
        else if (!new EmailAddressAttribute().IsValid(email))
        {
            ModelState.AddModelError(nameof(email), errorDescriber.InvalidEmail(email).Code);
        }
        else
        {
            AccountsUser? owner = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
            if (owner is { })
            {
                ModelState.AddModelError(nameof(email), errorDescriber.DuplicateEmail(email).Code);
            }
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(modelStateDictionary: ModelState);
        }

        return NoContent();

    }

    [HttpPost("validation/username")]
    public async Task<IActionResult> ValidateUserName([FromBody] ValidateUserNameRequestBody request,
        [FromServices] UserManager<AccountsUser> userManager,
        [FromServices] IdentityErrorDescriber errorDescriber)
    {
        string? userName = request.Value;
        UserOptions userOptions = userManager.Options.User;
        if (string.IsNullOrEmpty(userName))
        {
            ModelState.AddModelError(nameof(userName), errorDescriber.InvalidUserName(userName).Code);
        }
        else if (!string.IsNullOrEmpty(userOptions.AllowedUserNameCharacters)
            && userName.Any(c => !userOptions.AllowedUserNameCharacters.Contains(c)))
        {
            ModelState.AddModelError(nameof(userName), errorDescriber.InvalidUserName(userName).Code);
        }
        else
        {
            AccountsUser? owner = await userManager.FindByNameAsync(userName).ConfigureAwait(false);
            if (owner is { })
            {
                ModelState.AddModelError(nameof(userName), errorDescriber.DuplicateUserName(userName).Code);
            }
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(modelStateDictionary: ModelState);
        }

        return NoContent();
    }
    #endregion
}
