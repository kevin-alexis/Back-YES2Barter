using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.CreateObjeto;
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
    public class ObjetoController : BaseController<Objeto, ObjetoDTO, IObjetoService>
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly DataBaseContext _dbContext;
        private readonly IPropuestaIntercambioService _propuestaIntercambioService;
        private readonly ILogService _logService;
        public ObjetoController(IObjetoService service, 
            IPropuestaIntercambioService propuestaIntercambioService, 
            IMapper mapper, IWebHostEnvironment hostingEnvironment, 
            DataBaseContext dbContext,
             ILogService logService) : base(service, mapper, logService)
        {
            _hostingEnvironment = hostingEnvironment;
            _dbContext = dbContext;
            _propuestaIntercambioService = propuestaIntercambioService;
            _logService = logService;

        }

        [HttpPost("GetByName")]
        [Authorize(Roles = "Administrador, Intercambiador")]
        public async Task<IActionResult> GetByName([FromBody] string name)
        {
            var result = await _service.GetByName(name);
            return Ok(result);
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
            var ruta = _hostingEnvironment.ContentRootPath;
            try
            {
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
        public async Task<ActionResult> ActualizarObjeto([FromForm] CreateObjetoVM createObjetoVM, int idObjeto)
        {
            var ruta = _hostingEnvironment.ContentRootPath;
            var ObjetoDTO = _mapper.Map<ObjetoDTO>(createObjetoVM);
            try
            {
                if (createObjetoVM.RutaImagen != null)
                {
                    bool objetoEliminado = await _service.EliminarObjetoImagen(idObjeto, ruta);
                    var rutaImagen = await _service.GuardarObjetoImagen(createObjetoVM.IdCategoria, createObjetoVM.RutaImagen, ruta);

                    ObjetoDTO.RutaImagen = rutaImagen;
                    ObjetoDTO.Id = idObjeto;

                    await _service.Update(ObjetoDTO);
                    return Ok(new { success = true, message = "Objeto actualizado exitosamente" });

                    if (objetoEliminado)
                    {
                        rutaImagen = await _service.GuardarObjetoImagen(createObjetoVM.IdCategoria, createObjetoVM.RutaImagen, ruta);

                        ObjetoDTO.RutaImagen = rutaImagen;
                        ObjetoDTO.Id = idObjeto;

                        await _service.Update(ObjetoDTO);
                        return Ok(new { success = true, message = "Objeto actualizada exitosamente" });
                    }
                    else
                    {
                        return NotFound(new { success = false, message = "Objeto no encontrado" });
                    }
                }
                else
                {
                    var objeto = await _dbContext.Objetos.FirstOrDefaultAsync(c => c.Id == idObjeto);
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
                    return NotFound();
                }

                await _service.Delete(id);           

                return Ok();
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(Delete)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}
