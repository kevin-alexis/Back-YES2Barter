using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Contracts;
using Service.Services.Implementation;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonaController : BaseController<Persona, PersonaDTO, IPersonaService>
    {
        private readonly ILogService _logService;

        public PersonaController(IPersonaService service, IMapper mapper, ILogService logService) : base(service, mapper, logService)
        {
            _logService = logService;
        }

        [HttpGet("GetPersonaByIdEf/{id}")]
        public async Task<ActionResult<PersonaDTO>> GetPersonaByIdEf(int id)
        {
            try
            {
                var result = await _service.GetPersonaByIdEf(id);
                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetPersonaByIdEf)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
        }

        [HttpGet("GetPersonaByIdDapper/{id}")]
        public async Task<ActionResult<PersonaDTO>> GetPersonaByIdDapper(int id)
        {
            try
            {
                var result = await _service.GetPersonaByIdDapper(id);
                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetPersonaByIdDapper)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
        }

    }
}
