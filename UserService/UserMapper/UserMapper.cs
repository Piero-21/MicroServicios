using AutoMapper;
using UserService.Models;
using UserService.Models.Dtos;

namespace UserService.UserMapper
{
    public class UserMapper : Profile
    {
        public UserMapper()
        {
            CreateMap<User, RegisterUserDto>().ReverseMap();
        }
    }
}
