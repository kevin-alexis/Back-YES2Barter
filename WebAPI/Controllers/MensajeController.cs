using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.GetMensajes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Service.Services.Contracts;
using Service.Services.Implementation;
using System.Text.RegularExpressions;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MensajeController : BaseController<Mensaje, MensajeDTO, IMensajeService>
    {
        private readonly IMensajeService _service;
        private readonly ILogService _logService;
        public MensajeController(IMensajeService service, IMapper mapper, ILogService logService) : base(service, mapper, logService)
        {
            _service = service;
            _logService = logService;

        }

        [HttpGet("GetAllByIdChat/{idChat}")]
        [Authorize(Roles = "Administrador, Intercambiador")]
        public async Task<ActionResult<IEnumerable<GetMensajesVM>>> GetAllByIdChat(int idChat)
        {
            try
            {
                var itemsDto = await _service.GetAllByIdChat(idChat);
                return Ok(itemsDto);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetAllByIdChat)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }

}
