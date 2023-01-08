using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace Victa.Backend.Accounts.Infrastructure.Configuration.IdentityServer.Adapters.Validation;

public class ExtensionGrantValidator : IExtensionGrantValidator
{
    private readonly ILogger _logger;
    private readonly IEnumerable<IGrantHandler> _processors;

    public ExtensionGrantValidator(IEnumerable<IGrantHandler> processors,
        ILogger<ExtensionGrantValidator> logger)
    {
        _logger = logger;
        _processors = processors;
    }

    public string GrantType => GrantTypes.External;

    public async Task ValidateAsync(ExtensionGrantValidationContext context)
    {
        string? provider = context.Request.Raw.Get("provider");

        if (string.IsNullOrEmpty(provider))
        {
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "invalid_provider");
            return;
        }

        foreach (IGrantHandler? processor in _processors)
        {
            if (processor.IsSatisfiedBy(provider))
            {
                await processor.Handle(context);
                return;
            }
        }
    }
}
