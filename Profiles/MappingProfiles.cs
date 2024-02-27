using AutoMapper;
using Filmies_Data.Models;
using Filmzie.Models.Dto;

namespace Filmzie.Profiles
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<User, UserDTO>().ReverseMap();
        }
    }
}
