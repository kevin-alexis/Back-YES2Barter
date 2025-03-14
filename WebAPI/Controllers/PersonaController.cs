using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("GetAllPersonasIntercambiadores/")]
        [Authorize(Roles = "Administrador")]
        virtual public async Task<ActionResult<IEnumerable<PersonaDTO>>> GetAllPersonasIntercambiadores()
        {
            try
            {
                var itemsDto = await _service.GetAllPersonasIntercambiadores();
                return Ok(itemsDto);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetPersonaByIdEf)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("GetPersonaByIdUsuario/{idUsuario}")]
        public async Task<ActionResult<PersonaDTO>> GetPersonaByIdUsuario(string idUsuario)
        {
            try
            {
                var result = await _service.GetPersonaByIdUsuario(idUsuario);
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
                    Mensaje = $"Error en el método {nameof(GetPersonaByIdUsuario)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
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
