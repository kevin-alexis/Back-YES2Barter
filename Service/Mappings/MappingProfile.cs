using AutoMapper;
using Domain.Entities;
using Domain.DTOs;

namespace Service.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Persona, PersonaDTO>().ReverseMap();
        }
    }
}