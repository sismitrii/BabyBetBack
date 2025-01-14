using Application.Dtos;
using Application.Dtos.Common;
using Application.Dtos.In;
using Application.Dtos.Out;
using AutoMapper;
using Core.Entities;

namespace Application.Automapper;

public class ApplicationProfile : Profile
{
    public ApplicationProfile()
    {
        CreateMap<Bet, CreateUserBetRequest>()
            .ReverseMap();
        
        CreateMap<Bet, BetDto>()
            .ReverseMap();
        
        CreateMap<Name, NameDto>()
            .ReverseMap();
        
        CreateMap<User, UserDto>()
            .ReverseMap();
    }
}
