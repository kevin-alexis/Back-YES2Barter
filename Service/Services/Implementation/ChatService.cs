using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.GetChats;
using Domain.ViewModels.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Context;
using Service.Logging;
using Service.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class ChatService : BaseService<Chat, ChatDTO>, IChatService
    {
        private new readonly DataBaseContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        private readonly IPropuestaIntercambioService _propuestaIntercambioService;
        private readonly Logger _logger;

        public ChatService(DataBaseContext context, IMapper mapper, Logger logger, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager) : base(context, mapper, logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;

        }

        public async Task<EndpointResponse<List<GetChatsVM>>> GetAllByIdUsuario(string idUsuario)
        {
            if (idUsuario == null)
            {
                return new EndpointResponse<List<GetChatsVM>>
                {
                    Message = "El id es requerido",
                    Success = false,
                    Data = new List<GetChatsVM>()
                };
            }

            var result = await _context.Chats
                .Where(x => (x.IdUsuario1 == idUsuario || x.IdUsuario2 == idUsuario) && !x.EsBorrado)
                .ToListAsync();

            if (!result.Any())
            {
                return new EndpointResponse<List<GetChatsVM>>
                {
                    Message = "No se encontraron chats con ese usuario",
                    Success = false,
                    Data = new List<GetChatsVM>()
                };
            }
            var chatDTOs = new List<GetChatsVM>();

            foreach (var chat in result)
            {
                var personaEmisor = await _context.Personas.FirstOrDefaultAsync(p => p.IdUsuario == chat.IdUsuario1);
                var personaReceptor = await _context.Personas.FirstOrDefaultAsync(p => p.IdUsuario == chat.IdUsuario2);

                chatDTOs.Add(new GetChatsVM
                {
                    Id = chat.Id,
                    IdUsuario1 = chat.IdUsuario1,
                    PersonaEmisor = personaEmisor,
                    IdUsuario2 = chat.IdUsuario2,
                    PersonaReceptor = personaReceptor
                });
            }

            return new EndpointResponse<List<GetChatsVM>>
            {
                Message = "Chats obtenidos con éxito",
                Success = true,
                Data = chatDTOs
            };
        }
    }
}
