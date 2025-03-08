using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.Response;

namespace Service.Services.Contracts
{
    public interface IPersonaService : IBaseService<Persona, PersonaDTO>
    {
        Task<PersonaDTO> GetPersonaByIdDapper(int id);
        Task<PersonaDTO> GetPersonaByIdEf(int id);
    }
}
