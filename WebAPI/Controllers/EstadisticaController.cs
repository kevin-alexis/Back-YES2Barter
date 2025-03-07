using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Repository.Context;
using Service.Services.Contracts;
using Service.Services.Implementation;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstadisticaController : ControllerBase
    {
        private readonly IEstadisticaService _service;
        public EstadisticaController(IEstadisticaService service)
        {
            _service = service;
        }

        [HttpPost("GetEstadisticasByUser")]
        public async Task<IActionResult> GetEstadisticasByUser([FromBody] string token)
        {
            try
            {
                var result = await _service.GetEstadisticasByUser(token);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


    }
}
