using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.GetChats;
using Domain.ViewModels.Response;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Context;
using Service.Logging;
using Service.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static Domain.Enumerations.Enums;

namespace Service.Services.Implementation
{
    public class ChatService : BaseService<Chat, ChatDTO>, IChatService
    {
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        private readonly IPropuestaIntercambioService _propuestaIntercambioService;
        private readonly ILogService _logService;

        public ChatService(
            DataBaseContext context, 
            IMapper mapper, 
            ILogService logService,
            Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager, 
            IPropuestaIntercambioService propuestaIntercambioService) : base(context, mapper, logService)
        {
            _userManager = userManager;
            _logService = logService;
            _propuestaIntercambioService = propuestaIntercambioService;
        }

        public async Task<EndpointResponse<List<GetChatsVM>>> GetAllByIdUsuario(string idUsuario)
        {
            try
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
                    var propuestasIntercambios = await _context.PropuestasIntercambios.FirstOrDefaultAsync(p => p.Id == chat.IdPropuestaIntercambio);

                    chatDTOs.Add(new GetChatsVM
                    {
                        Id = chat.Id,
                        IdUsuario1 = chat.IdUsuario1,
                        PersonaEmisor = personaEmisor,
                        IdUsuario2 = chat.IdUsuario2,
                        PersonaReceptor = personaReceptor,
                        IdPropuestaIntercambio = chat.IdPropuestaIntercambio,
                        PropuestaIntercambio = propuestasIntercambios
                    });
                }

                return new EndpointResponse<List<GetChatsVM>>
                {
                    Message = "Chats obtenidos con éxito",
                    Success = true,
                    Data = chatDTOs
                };
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetAllByIdUsuario)}, de la clase {nameof(ChatService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
        }

        public async Task CloseChat(int idChat, bool isSuccess)
        {
            try
            {
                var chat = await _context.Chats.FirstOrDefaultAsync(x => x.Id == idChat);

                if(chat == null)
                {
                    throw new HubException("No se pudo obtener el chat.");
                }
                var idPropuestaIntercambio = chat.IdPropuestaIntercambio;
                var estatus = isSuccess ? EstatusPropuestaIntercambio.CONCRETADA : EstatusPropuestaIntercambio.NO_CONCRETADA;
                // mando a hacer el cambio de estatus de la propuesta y objetos
                await _propuestaIntercambioService.ChangeStatus(idPropuestaIntercambio, estatus);

                chat.EsBorrado = true;
                _dbSet.Update(chat);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(CloseChat)}, de la clase {nameof(ChatService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw new Exception("Error al actualizar el elemento", ex);
            }
        }
    }
}
