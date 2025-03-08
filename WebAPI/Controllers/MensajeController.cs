using Microsoft.AspNetCore.Mvc;
using Service.Services;
using Domain.DTOs;

namespace WebAPI.Controllers
{
    [Route("api/mensajes")]
    [ApiController]
    public class MensajeController : ControllerBase
    {
        private readonly IMensajeService _mensajeService;

        public MensajeController(IMensajeService mensajeService)
        {
            _mensajeService = mensajeService;
        }

        [HttpGet("{emisorId}/{receptorId}")]
        public async Task<IActionResult> ObtenerMensajes(int emisorId, int receptorId)
        {
            var mensajes = await _mensajeService.ObtenerMensajesAsync(emisorId, receptorId);
            return Ok(mensajes);
        }

        [HttpPost]
        public async Task<IActionResult> EnviarMensaje([FromBody] MensajeDTO mensaje)
        {
            await _mensajeService.EnviarMensajeAsync(mensaje);
            return Ok();
        }
    }
}
