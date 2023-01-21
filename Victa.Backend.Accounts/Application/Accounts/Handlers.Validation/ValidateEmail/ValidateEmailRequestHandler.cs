using FluentValidation;

using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Validation.ValidateEmail;

public sealed class ValidateEmailRequestHandler
    : IRequestHandler<ValidateEmailRequest, ValidateEmailResponse>
{
    private readonly ILogger _logger;
    private readonly IValidator<ValidateEmailRequest> _validator;

    public ValidateEmailRequestHandler(
        ILogger<ValidateEmailRequestHandler> logger,
        IValidator<ValidateEmailRequest> validator)
    {
        _logger = logger;
        _validator = validator;
    }

    public async Task<ValidateEmailResponse> Handle(ValidateEmailRequest request,
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
                "Unable to validate email (Email={value})",
                request.Email);

            throw;
        }

        return ValidateEmailResponse.Success(result);
    }
}
