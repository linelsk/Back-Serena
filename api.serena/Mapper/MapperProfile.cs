using AutoMapper;
using api.premier.Models;
using biz.serena.Entities;


namespace api.premier.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            #region User
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<User, UserCreateDto>().ReverseMap();
            CreateMap<User, UserUpdateDto>().ReverseMap();
            #endregion

        }
    }
}
