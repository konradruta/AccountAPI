using AccountAPI.Entities;
using AccountAPI.Models;
using AutoMapper;

namespace AccountAPI
{
    public class AccountMappingProfile : Profile
    {
        public AccountMappingProfile()
        {
            CreateMap<Account, AccountDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Name));

            CreateMap<EditAccountDto, Account>()
                .ForMember(dest => dest.Name, opt => opt.Condition(src => src.Name != null))
                .ForMember(dest => dest.RoleId,
                    opt => opt.Ignore());
        }
    }
}
