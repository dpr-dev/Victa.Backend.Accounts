using IdentityServer4.Validation;

namespace Victa.Backend.Accounts.Infrastructure.Configuration.IdentityServer.Adapters.Validation;

public interface IGrantHandler
{
    bool IsSatisfiedBy(string provider);

    Task Handle(ExtensionGrantValidationContext context);
}
