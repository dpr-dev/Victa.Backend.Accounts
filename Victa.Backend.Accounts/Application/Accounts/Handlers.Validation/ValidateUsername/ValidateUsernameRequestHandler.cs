using FluentValidation;

using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Validation.ValidateUsername;

public sealed class ValidateUsernameRequestHandler
    : IRequestHandler<ValidateUsernameRequest, ValidateUsernameResponse>
{
    private readonly ILogger _logger;
    private readonly IValidator<ValidateUsernameRequest> _validator;

    public ValidateUsernameRequestHandler(
        ILogger<ValidateUsernameRequestHandler> logger, 
        IValidator<ValidateUsernameRequest> validator)
    {
        _logger = logger;
        _validator = validator;
    }

    public async Task<ValidateUsernameResponse> Handle(ValidateUsernameRequest request,
        CancellationToken cancellationToken)
    {
        FluentValidation.Results.ValidationResult result;

        try
        {
            result = await _validator.ValidateAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unable to validate username (UserName={value})",
                request.Username); 

            throw;
        }

        return ValidateUsernameResponse.Success(result);
    }
}
