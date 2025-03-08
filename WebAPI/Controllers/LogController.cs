using AutoMapper;
using Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Contracts;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly ILogService _service;
        public LogController(ILogService service, IMapper mapper)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        virtual public async Task<ActionResult<IEnumerable<LogDTO>>> GetAll()
        {
            try
            {
                var itemsDto = await _service.GetAll();
                return Ok(itemsDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        virtual public async Task<ActionResult> Add([FromBody] LogDTO logDTO)
        {
            try
            {
                await _service.Add(logDTO);
                return CreatedAtAction(nameof(GetById), new { id = logDTO.GetHashCode() }, logDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador")]
        virtual public async Task<ActionResult<LogDTO>> GetById(int id)
        {
            try
            {
                var itemDto = await _service.GetById(id);
                if (itemDto == null)
                {
                    return NotFound();
                }
                return Ok(itemDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

    }
}
