using System.Net;

using FluentValidation.Results;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Victa.Backend.Accounts.Application.Accounts.Handlers.Configure;
using Victa.Backend.Accounts.Application.Accounts.Handlers.GetMe;
using Victa.Backend.Accounts.Application.Accounts.Handlers.Logout;
using Victa.Backend.Accounts.Application.Accounts.Handlers.Password.ChangePassword;
using Victa.Backend.Accounts.Application.Accounts.Handlers.Password.CreateRecoveryCode;
using Victa.Backend.Accounts.Application.Accounts.Handlers.Password.HandleRecoveryCode;
using Victa.Backend.Accounts.Application.Accounts.Handlers.Password.VerifyPassword;
using Victa.Backend.Accounts.Application.Accounts.Handlers.Registration.RegisterViaPassword;
using Victa.Backend.Accounts.Application.Accounts.Handlers.RequestDeletion;
using Victa.Backend.Accounts.Application.Accounts.Handlers.Validation.ValidateEmail;
using Victa.Backend.Accounts.Application.Accounts.Handlers.Validation.ValidateUsername;

using Victa.Backend.Accounts.Contracts.Input.Accounts;
using Victa.Backend.Accounts.Contracts.Input.Accounts.Validation;

using Victa.Backend.Accounts.Core.AspNetCore.Authorization;
using Victa.Backend.Accounts.Core.AspNetCore.Mvc;

namespace Victa.Backend.Accounts.Controllers.Account;

[ApiController]
[Route("api/v1/account")]
public sealed class AccountController : ApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IMediator mediator, ILogger<AccountController> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unable to process request");

            return Problem(statusCode: HttpStatusCode.InternalServerError);
        }

        return result.Match(Json, Error);
    }

    [HttpDelete]
    [HttpPost("request-deletion")]
    [AuthorizeCustomer]
    public async Task<IActionResult> RequestDeletion()
    {
        RequestDeletionResponse result;
        try
        {
            result = await _mediator.Send(new RequestDeletionRequest(UserId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unable to process request");

            throw;
        }

        return result.Match(_ => NoContent(), Error);
    }

    [HttpPost("configure")]
    [AuthorizeCustomer]
    public async Task<IActionResult> Configure([FromBody] ConfigureBody body)
    {
        ConfigureResponse result;
        try
        {
            result = await _mediator.Send(new ConfigureRequest(UserId, body.FirebaseToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to process request");

            throw;
        }

        return result.Match(_ => NoContent(), Error);
    }

    [HttpPost("logout")]
    [AuthorizeCustomer]
    public async Task<IActionResult> Logout([FromBody] LogoutBody body)
    {
        LogoutResponse result;
        try
        {
            result = await _mediator.Send(new LogoutRequest(UserId, body.FirebaseToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unable to process request");

            throw;
        }

        return result.Match(_ => NoContent(), Error);
    }

    [AuthorizeCustomer]
    [HttpPut("password")]
    public async Task<IActionResult> UpdatePassword([FromBody] ChangePasswordBody body)
    {
        ChangePasswordResponse result;
        try
        {
            result = await _mediator.Send(new ChangePasswordRequest(UserId, body.Password));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unable to process request");

            throw;
        }

        return result.Match(_ => NoContent(), Error);
    }

    [AuthorizeCustomer]
    [HttpPost("password/verification")]
    public async Task<IActionResult> ValidatePassword([FromBody] VerifyPasswordBody body)
    {
        VerifyPasswordResponse result;
        try
        {
            result = await _mediator.Send(new VerifyPasswordRequest(UserId, body.Password));
        }
        catch (Exception)
        {
            return Problem(statusCode: HttpStatusCode.InternalServerError);
        }

        return result.Match(_ => NoContent(), Error);
    }



    [HttpPost("password/forgot")]
    public async Task<IActionResult> ForgotPassword([FromBody] CreateRecoveryCodeBody body)
    {
        CreateRecoveryCodeResponse result;
        try
        {
            result = await _mediator.Send(new CreateRecoveryCodeRequest(body.Email));
        }
        catch (Exception)
        {
            return Problem(statusCode: HttpStatusCode.InternalServerError);
        }

        return result.Match(_ => NoContent(), Error);
    }

    [HttpPost("password/restore")]
    public async Task<IActionResult> ResotrePassword([FromBody] RestorePasswordWithRecoveryCodeBody body)
    {
        HandleRecoveryCodeResponse result;
        try
        {
            result = await _mediator.Send(new HandleRecoveryCodeRequest(body.Email, body.RecoveryCode, body.Password));
        }
        catch (Exception)
        {
            return Problem(statusCode: HttpStatusCode.InternalServerError);
        }

        return result.Match(_ => NoContent(), Error);
    }



    #region Registration
    [AllowAnonymous]
    [HttpPost("register/password")]
    public async Task<IActionResult> Register([FromBody] PasswordRegistrationBody source)
    {
        RegisterViaPasswordResponse result;
        try
        {
            result = await _mediator.Send(new RegisterViaPasswordRequest(source));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unable to process request");

            throw;
        }

        return result.Match(Json, Error);
    }
    #endregion


    #region Validation
    [HttpPost("validation/email")]
    public async Task<IActionResult> ValidateEmail([FromBody] ValidateEmailRequestBody request)
    {
        ValidateEmailResponse result;
        try
        {
            result = await _mediator.Send(new ValidateEmailRequest(request.Value));
        }
        catch (Exception)
        {
            throw;
        }

        return result.Match(HandleValidationResult, Error);
    }

    [HttpPost("validation/username")]
    public async Task<IActionResult> ValidateUserName([FromBody] ValidateUserNameRequestBody request)
    {
        ValidateUsernameResponse result;
        try
        {
            result = await _mediator.Send(new ValidateUsernameRequest(request.Value));
        }
        catch (Exception)
        {
            throw;
        }

        return result.Match(HandleValidationResult, Error);
    }


    private IActionResult HandleValidationResult(ValidationResult result)
    {
        if (result.IsValid)
        {
            return NoContent();
        }

        foreach (ValidationFailure error in result.Errors)
        {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        return ValidationProblem(ModelState);
    }
    #endregion
}
