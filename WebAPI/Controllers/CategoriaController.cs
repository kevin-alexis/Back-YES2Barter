using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Contracts;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : BaseController<Categoria, CategoriaDTO, ICategoriaService>
    {
        private readonly ILogService _logService;
        public CategoriaController(ICategoriaService service, IMapper mapper, ILogService logService) : base(service, mapper, logService)
        {
            _logService = logService;

        }
        [Authorize(Roles = "Administrador")]
        override public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var categoriaDto = await _service.GetById(id);
                if (categoriaDto == null)
                {
                    return NotFound(new { message = "La categoría no existe." });
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
                    Mensaje = $"Error en {nameof(Delete)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });

                return StatusCode(500, new { success = false, message = $"Error interno del servidor: {ex.Message}" });
            }
        }


    }
}
