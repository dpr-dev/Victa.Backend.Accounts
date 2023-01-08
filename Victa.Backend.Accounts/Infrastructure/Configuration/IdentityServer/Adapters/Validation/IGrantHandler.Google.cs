using IdentityServer4.Validation;

using Victa.Backend.Accounts.Infrastructure.Configuration.IdentityServer.Adapters.Validation;

namespace Victa.Backend.Infrastructure.IdentityServer.Validation;

public class GoogleGrantHandler : IGrantHandler
{
    public string Provider => "google";

    public async Task Handle(ExtensionGrantValidationContext context)
    {
        throw new NotImplementedException(nameof(Handle));
    }

    public bool IsSatisfiedBy(string provider)
    {
        return string.Equals(provider, Provider, StringComparison.OrdinalIgnoreCase);
    }
}
