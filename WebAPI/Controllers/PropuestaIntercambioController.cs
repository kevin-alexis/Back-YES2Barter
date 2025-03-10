using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.CreatePropuestaIntercambio;
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

        [HttpGet("{id}")]
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
