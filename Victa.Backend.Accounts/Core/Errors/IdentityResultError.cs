using Microsoft.AspNetCore.Identity;

namespace Victa.Backend.Accounts.Core.Errors;

public class IdentityResultError : ExecutionError
{
    public IdentityResultError(IEnumerable<IdentityError> errors)
    {
        if (errors is null)
        {
            throw new ArgumentNullException(nameof(errors));
        }

        Errors = errors;
    }

    public IEnumerable<IdentityError> Errors { get; }
}
