using IdentityServer4.Validation;

using Victa.Backend.Accounts.Infrastructure.Configuration.IdentityServer.Adapters.Validation;

namespace Victa.Backend.Infrastructure.IdentityServer.Validation;

public class AppleGrantHandler : IGrantHandler
{
    public string Provider => "apple";

    public Task Handle(ExtensionGrantValidationContext context)
    {
        throw new NotImplementedException();
    }

    public bool IsSatisfiedBy(string provider)
    {
        throw new NotImplementedException();
    }
}
