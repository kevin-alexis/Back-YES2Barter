using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.Response;

namespace Service.Services.Contracts
{
    public interface IPersonaService : IBaseService<Persona, PersonaDTO>
    {
        Task<IEnumerable<PersonaDTO>> GetAllPersonasIntercambiadores();
        Task<PersonaDTO> GetPersonaByIdDapper(int id);
        Task<PersonaDTO> GetPersonaByIdEf(int id);
        Task<PersonaDTO> GetPersonaByEmailDapper(string email);
        Task<PersonaDTO> GetPersonaByIdUsuario(string idUsuario);
        Task<PersonaDTO> UpdatePersona(int id, PersonaDTO personaDto);

    }
}
