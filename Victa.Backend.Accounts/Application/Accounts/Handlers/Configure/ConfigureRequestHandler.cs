using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Configure;

public sealed class ConfigureRequestHandler
    : IRequestHandler<ConfigureRequest, ConfigureResponse>
{
    public Task<ConfigureResponse> Handle(ConfigureRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
