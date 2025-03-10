using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.GetChats;
using Domain.ViewModels.GetMensajes;
using Domain.ViewModels.Response;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using Service.Logging;
using Service.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class MensajeService : BaseService<Mensaje, MensajeDTO>, IMensajeService
    {
        private new readonly DataBaseContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        private readonly Logger _logger;
        private readonly IChatService _chatService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MensajeService(
            DataBaseContext context, 
            IMapper mapper, Logger logger, 
            Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager, 
            IChatService chatService,
            IHttpContextAccessor httpContextAccessor
            ) : base(context, mapper, logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _chatService = chatService;
            _httpContextAccessor = httpContextAccessor;

        }

        public async Task GuardarMensaje(int idChat, string idEmisor, string idReceptor, string contenido)
        {
            var mensaje = new Mensaje
            {
                IdChat = idChat,
                IdUsuarioEmisor = idEmisor,
                IdUsuarioReceptor = idReceptor,
                Contenido = contenido,
                FechaEnvio = DateTime.Now
            };

            _context.Mensajes.Add(mensaje);
            await _context.SaveChangesAsync();
        }

        public async Task<EndpointResponse<List<GetMensajesVM>>> GetAllByIdChat(int idChat)
        {
            if (idChat <= 0)
            {
                return new EndpointResponse<List<GetMensajesVM>>
                {
                    Message = "El id es requerido",
                    Success = false,
                    Data = new List<GetMensajesVM>()
                };
            }

            var chat = await _context.Chats.FirstOrDefaultAsync(x => x.Id == idChat && x.EsBorrado == false);
            if (chat == null)
            {
                return new EndpointResponse<List<GetMensajesVM>>
                {
                    Message = "Chat no encontrado",
                    Success = false,
                    Data = new List<GetMensajesVM>()
                };
            }

            var claims = _httpContextAccessor.HttpContext?.User.Claims.ToList();
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst("uid")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return new EndpointResponse<List<GetMensajesVM>>
                {
                    Message = "Usuario no autenticado",
                    Success = false,
                    Data = new List<GetMensajesVM>()
                };
            }

            if (chat.IdUsuario1 != userId && chat.IdUsuario2 != userId)
            {
                return new EndpointResponse<List<GetMensajesVM>>
                {
                    Message = "El usuario no tiene permisos para acceder a este chat.",
                    Success = false,
                    Data = new List<GetMensajesVM>()
                };
            }

            var result = await _context.Mensajes
                .Where(x => x.IdChat == idChat && x.EsBorrado == false)
                .OrderBy(m => m.FechaEnvio)
                .ToListAsync();

            if (!result.Any())
            {
                return new EndpointResponse<List<GetMensajesVM>>
                {
                    Message = "No se encontraron mensajes con ese chat",
                    Success = false,
                    Data = new List<GetMensajesVM>()
                };
            }

            var mensajeDTOs = new List<GetMensajesVM>();

            foreach (var mensaje in result)
            {
                var personaEmisor = await _context.Personas.FirstOrDefaultAsync(p => p.IdUsuario == mensaje.IdUsuarioEmisor);
                var personaReceptor = await _context.Personas.FirstOrDefaultAsync(p => p.IdUsuario == mensaje.IdUsuarioReceptor);

                mensajeDTOs.Add(new GetMensajesVM
                {
                    Id = mensaje.Id,
                    IdChat = mensaje.IdChat,
                    Contenido = mensaje.Contenido,
                    IdUsuarioEmisor = mensaje.IdUsuarioEmisor,
                    PersonaEmisor = personaEmisor,
                    IdUsuarioReceptor = mensaje.IdUsuarioReceptor,
                    PersonaReceptor = personaReceptor,
                    FechaEnvio = mensaje.FechaEnvio,
                });
            }

            return new EndpointResponse<List<GetMensajesVM>>
            {
                Message = "Mensajes obtenidos con éxito",
                Success = true,
                Data = mensajeDTOs
            };
        }

    }
}
