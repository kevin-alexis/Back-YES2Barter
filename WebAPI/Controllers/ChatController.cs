using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.GetChats;
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
    public class ChatController : BaseController<Chat, ChatDTO, IChatService>
    {
        private readonly IChatService _service;
        private readonly IMensajeService _mensajeService;
        public ChatController(IChatService service, IMensajeService mensajeService, IMapper mapper) : base(service, mapper)
        {
            _service = service;
            _mensajeService = mensajeService;
        }

        [HttpGet("GetAllByIdUsuario/{idUsuario}")]
        public async Task<ActionResult<IEnumerable<GetChatsVM>>> GetAllByIdUsuario(string idUsuario)
        {
            try
            {
                var itemsDto = await _service.GetAllByIdUsuario(idUsuario);
                return Ok(itemsDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

    }
}
