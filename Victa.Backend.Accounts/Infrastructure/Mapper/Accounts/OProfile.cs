using AutoMapper;

using Victa.Backend.Accounts.Contracts.Output.Accounts;
using Victa.Backend.Accounts.Domain.Models.UserAggregate;

namespace Victa.Backend.Accounts.Infrastructure.Mapper.Accounts;

public class OProfile : Profile
{
    public OProfile()
    {
        _ = CreateMap<AccountsUser, OAccountsUser>();
    }
}
