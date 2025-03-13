using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.CreatePropuestaIntercambio;
using Domain.ViewModels.EditPropuestaIntercambio;
using Domain.ViewModels.GetPropuestasIntercambios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using Service.Services.Contracts;
using Service.Services.Implementation;



namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropuestaIntercambioController : BaseController<PropuestaIntercambio, PropuestaIntercambioDTO, IPropuestaIntercambioService>
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly DataBaseContext _dbContext;
        private readonly ILogService _logService;
        public PropuestaIntercambioController(
            IPropuestaIntercambioService service, 
            IMapper mapper, 
            IWebHostEnvironment hostingEnvironment, 
            DataBaseContext dbContext,
            ILogService logService
            ) : base(service, mapper, logService)
        {
            _hostingEnvironment = hostingEnvironment;
            _dbContext = dbContext;
            _logService = logService;
        }


        [HttpPost("CreatePropuestaIntercambio")]
        [Authorize(Roles = "Administrador, Intercambiador")]
        public async Task<IActionResult> AddPropuesta([FromBody] CreatePropuestaIntercambioVM createPropuestaIntercambioVM)
        {
            try
            {
                var result = await _service.AddPropuesta(createPropuestaIntercambioVM);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(AddPropuesta)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPut("UpdatePropuestaIntercambio/{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> UpdatePropuesta([FromBody] EditPropuestaIntercambioVM editPropuestaIntercambioVM, int id)
        {
            try
            {
                var result = await _service.UpdatePropuesta(id, editPropuestaIntercambioVM);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(UpdatePropuesta)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpDelete("DeletePropuestaIntercambio/{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeletePropuesta(int id)
        {
            try
            {
                var result = await _service.DeletePropuesta(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(DeletePropuesta)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


        [HttpGet("GetAllPropuestas")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<List<PropuestasIntercambiosVM>>> GetAllPropuestas()
        {
            try
            {
                var itemsDto = await _service.GetAllPropuestas();
                return Ok(itemsDto);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetAllPropuestas)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador, Intercambiador")]
        override public async Task<ActionResult<PropuestaIntercambioDTO>> GetById(int id)
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
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetById)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("GetAllByIdObjeto/{idObjeto}")]
        [Authorize(Roles = "Administrador, Intercambiador")]
        public async Task<ActionResult<IEnumerable<PropuestaIntercambioDTO>>> GetAllByIdObjeto(int idObjeto)
        {
            try
            {
                var itemsDto = await _service.GetAllByIdObjeto(idObjeto);
                return Ok(itemsDto);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetAllByIdObjeto)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}
