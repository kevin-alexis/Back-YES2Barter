using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.CreateObjeto;
using Domain.ViewModels.EditObjeto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using Service.Services.Contracts;
using Service.Services.Implementation;
using System.Security.Claims;
using static Domain.Enumerations.Enums;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ObjetoController : BaseController<Objeto, ObjetoDTO, IObjetoService>
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly DataBaseContext _dbContext;
        private readonly IPropuestaIntercambioService _propuestaIntercambioService;
        private readonly ILogService _logService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ObjetoController(IObjetoService service, 
            IPropuestaIntercambioService propuestaIntercambioService, 
            IMapper mapper, IWebHostEnvironment hostingEnvironment, 
            DataBaseContext dbContext,
            IHttpContextAccessor httpContextAccessor,
             ILogService logService) : base(service, mapper, logService)
        {
            _hostingEnvironment = hostingEnvironment;
            _dbContext = dbContext;
            _propuestaIntercambioService = propuestaIntercambioService;
            _logService = logService;
            _httpContextAccessor = httpContextAccessor;

        }

        [HttpPost("GetAllByIdEstatus")]
        [Authorize(Roles = "Administrador, Intercambiador")]
        public async Task<IActionResult> GetAllByIdEstatus([FromBody] EstatusObjeto? estatus)
        {
            var result = await _service.GetAllByIdEstatus(estatus);
            return Ok(result);
        }

        [HttpPost("GetByName")]
        [Authorize(Roles = "Administrador, Intercambiador")]
        public async Task<IActionResult> GetByName([FromBody] string name)
        {
            var result = await _service.GetByName(name);
            return Ok(result);
        }

        [HttpGet("GetAllByIdUsuario/{idUsuario}")]
        [Authorize(Roles = "Administrador, Intercambiador")]
        public async Task<ActionResult<IEnumerable<ObjetoDTO>>> GetAllByIdUsuario(string idUsuario)
        {
            try
            {
                var itemsDto = await _service.GetAllByIdUsuario(idUsuario);
                return Ok(itemsDto);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetAllByIdUsuario)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("GetAllByIdCategoria/{idCategoria}")]
        [Authorize(Roles = "Administrador, Intercambiador")]
        public async Task<ActionResult<IEnumerable<ObjetoDTO>>> GetAllByIdCategoria(int idCategoria)
        {
            try
            {
                var itemsDto = await _service.GetAllByIdCategoria(idCategoria);
                return Ok(itemsDto);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetAllByIdCategoria)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("create-objeto")]
        [Authorize(Roles = "Administrador, Intercambiador")]
        public async Task<ActionResult> CreateObjeto([FromForm] CreateObjetoVM createObjetoVM)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext?.User.FindFirst("uid")?.Value;
                var userRole = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

                createObjetoVM.Estado = EstatusObjeto.DISPONIBLE;
                createObjetoVM.FechaPublicacion = DateTime.Now;

                if (userRole == "Intercambiador" && userId != null)
                {
                    createObjetoVM.IdUsuario = userId;
                }

                var ruta = _hostingEnvironment.ContentRootPath;
                var rutaObjeto = await _service.GuardarObjetoImagen(createObjetoVM.IdCategoria, createObjetoVM.RutaImagen, ruta);

                var ObjetoDTO = _mapper.Map<ObjetoDTO>(createObjetoVM);
                ObjetoDTO.RutaImagen = rutaObjeto;

                await _service.Add(ObjetoDTO);
                return Ok(new { success = true, message = "Objeto creado exitosamente" });
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(CreateObjeto)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPut("update-objeto/{idObjeto}")]
        [Authorize(Roles = "Administrador, Intercambiador")]
        public async Task<ActionResult> ActualizarObjeto([FromForm] EditObjetoVM editObjetoVM, int idObjeto)
        {
            try
            {
                var ruta = _hostingEnvironment.ContentRootPath;
                var ObjetoDTO = _mapper.Map<ObjetoDTO>(editObjetoVM);
                var objeto = await _dbContext.Objetos.FirstOrDefaultAsync(o => o.Id == idObjeto);
                ObjetoDTO.FechaPublicacion = objeto.FechaPublicacion;

                var userId = _httpContextAccessor.HttpContext?.User.FindFirst("uid")?.Value;
                var userRole = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

                ObjetoDTO.Estado = objeto.Estado;
                ObjetoDTO.IdUsuario = objeto.IdUsuario;

                if (editObjetoVM.RutaImagen != null)
                {
                    bool objetoEliminado = await _service.EliminarObjetoImagen(idObjeto, ruta);
                    var rutaImagen = await _service.GuardarObjetoImagen(editObjetoVM.IdCategoria, editObjetoVM.RutaImagen, ruta);
                    ObjetoDTO.RutaImagen = rutaImagen;
                    ObjetoDTO.Id = idObjeto;
                    await _service.Update(ObjetoDTO);
                    return Ok(new { success = true, message = "Objeto actualizado exitosamente" });
                }
                else
                {
                    ObjetoDTO.RutaImagen = objeto.RutaImagen;
                    ObjetoDTO.Id = idObjeto;
                    await _service.Update(ObjetoDTO);
                    return Ok(new { success = true, message = "Objeto actualizado exitosamente" });
                }
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(ActualizarObjeto)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [Authorize(Roles = "Administrador, Intercambiador")]
        override public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var itemDto = await _service.GetById(id);
                if (itemDto == null)
                {
                    return NotFound(new { success = false, message = "El objeto no existe." });
                }

                var response = await _service.Delete(id);

                if (!response.Success)
                {
                    return BadRequest(new { success = false, message = response.Message });
                }


                return Ok(new { success = true, message = response.Message });

            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(Delete)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });

                return StatusCode(500, new { success = false, message = $"Error interno del servidor: {ex.Message}" });
            }

        }

    }
}
