using AutoMapper;
using Domain.Entities;
using Domain.DTOs;
using Domain.ViewModels.CreateObjeto;
using Domain.ViewModels.CreatePropuestaIntercambio;

namespace Service.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Entidades del Proyecto
            CreateMap<Objeto, ObjetoDTO>().ReverseMap();
            CreateMap<Categoria, CategoriaDTO>().ReverseMap();
            CreateMap<PropuestaIntercambio, PropuestaIntercambioDTO>().ReverseMap();
            CreateMap<Persona, PersonaDTO>().ReverseMap();
            CreateMap<Chat, ChatDTO>().ReverseMap();
            CreateMap<Mensaje, MensajeDTO>().ReverseMap();
            CreateMap<Log, LogDTO>().ReverseMap();

            CreateMap<CreateObjetoVM, ObjetoDTO>()
            .ForMember(dest => dest.RutaImagen, opt => opt.Ignore());
        }
    }
}