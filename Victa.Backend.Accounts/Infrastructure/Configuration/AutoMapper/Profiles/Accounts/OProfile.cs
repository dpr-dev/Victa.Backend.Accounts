using AutoMapper;

using Victa.Backend.Accounts.Contracts.Output.Accounts;
using Victa.Backend.Accounts.Domain.Models.UserAggregate;

namespace Victa.Backend.Accounts.Infrastructure.Configuration.AutoMapper.Profiles.Accounts;

public class OProfile : Profile
{
    public OProfile()
    {
        _ = CreateMap<AccountsUser, OAccountsUser>();
        _ = CreateMap<CompletionSteps, OCompletionSteps>();
    }
}
