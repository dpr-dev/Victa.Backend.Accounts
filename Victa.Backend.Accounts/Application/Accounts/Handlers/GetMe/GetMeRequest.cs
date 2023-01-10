using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.GetMe;


public sealed class GetMeRequest : IRequest<GetMeResponse>
{
    public GetMeRequest(string id)
    {
        if (id is null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        Id = id;
    }

    public string Id { get; }
}
