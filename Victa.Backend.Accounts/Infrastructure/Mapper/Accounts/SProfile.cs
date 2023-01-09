using AutoMapper;
using AutoMapper.Extensions.EnumMapping;

using Victa.Backend.Accounts.Contracts;
using Victa.Backend.Accounts.Domain.Models.UserAggregate;

namespace Victa.Backend.Accounts.Infrastructure.Mapper.Accounts;

public class SProfile : Profile
{
    public SProfile()
    {
        _ = CreateMap<SGender, Gender>()
            .ConvertUsingEnumMapping(cfg => cfg.MapByName());
    }
}
