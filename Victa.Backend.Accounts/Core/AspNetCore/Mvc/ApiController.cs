using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Claims;

using IdentityModel;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Victa.Backend.Accounts.Core.Errors;

namespace Victa.Backend.Accounts.Core.AspNetCore.Mvc;

public class ApiController : ControllerBase
{
    public string UserId =>
        User.FindFirstValue(JwtClaimTypes.Subject)
        ?? throw new InvalidOperationException($"Unable to resolve {JwtClaimTypes.Subject}");

    [NonAction]
    protected virtual StatusCodeResult StatusCode(HttpStatusCode statusCode)
    {
        return StatusCode((int)statusCode);
    }

    [NonAction]
    protected virtual ObjectResult StatusCode(HttpStatusCode statusCode, object value)
    {
        return StatusCode((int)statusCode, value);
    }

    [NonAction]
    protected virtual IActionResult Json(object? data)
    {
        return new JsonResult(data);
    }

    [NonAction]
    protected virtual IActionResult Json(object? data, object serializerSettings)
    {
        return new JsonResult(data, serializerSettings);
    }

    [NonAction]
    protected virtual ObjectResult Problem(string? detail = null, string? instance = null, HttpStatusCode? statusCode = null, string? title = null, string? type = null)
    {
        return Problem(detail, instance, (int?)statusCode, title, type);
    }

    [NonAction]
    protected virtual IActionResult Error(ExecutionError ex)
    {
        if (ex is IdentityResultError
            { Errors: var identityErrors })
        {
            foreach (IdentityError error in identityErrors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return ValidationProblem(type: ex.Code, detail: ex.Details, modelStateDictionary: ModelState);
        }

        return Problem(type: ex.Code, detail: ex.Details, statusCode: ex switch
        {
            NotFoundError => HttpStatusCode.NotFound,
            DuplicateError => HttpStatusCode.Conflict,
            _ => HttpStatusCode.InternalServerError
        });
    }



    protected bool LogError(ILogger logger, Exception ex, [CallerMemberName] string? methodName = null)
    {
        logger.LogError(ex,
            "[{method}] Error occured during request processing",
            methodName);

        return true;
    }
}
