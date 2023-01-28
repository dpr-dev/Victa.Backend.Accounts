using AutoMapper;
using AutoMapper.Extensions.EnumMapping;

using Victa.Backend.Accounts.Contracts;
using Victa.Backend.Accounts.Domain.Models.UserAggregate;

namespace Victa.Backend.Accounts.Infrastructure.AutoMapper.Profiles.Accounts;

public class SProfile : Profile
{
    public SProfile()
    {
        _ = CreateMap<SGender, Gender>()
            .ConvertUsingEnumMapping(cfg => cfg.MapByName());
    }
}
