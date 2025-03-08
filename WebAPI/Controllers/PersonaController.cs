using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Contracts;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonaController : BaseController<Persona, PersonaDTO, IPersonaService>
    {
        public PersonaController(IPersonaService service, IMapper mapper) : base(service, mapper)
        {
        }

        [HttpGet("GetPersonaByIdEf/{id}")]
        public async Task<ActionResult<PersonaDTO>> GetPersonaByIdEf(int id)
        {
            var result = await _service.GetPersonaByIdEf(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("GetPersonaByIdDapper/{id}")]
        public async Task<ActionResult<PersonaDTO>> GetPersonaByIdDapper(int id)
        {
            var result = await _service.GetPersonaByIdDapper(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

    }
}
