using Domain.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Repository.Context;
using Service.Services.Contracts;
using Service.Services.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.SignalR
{
    public class ChatHub : Hub<IChatClient>
    {
        private readonly IMensajeService _mensajeService;
        private readonly IChatService _chatService;
        private readonly ILogService _logService;

        public ChatHub(IMensajeService mensajeService, IChatService chatService, ILogService logService)
        {
            _mensajeService = mensajeService;
            _chatService = chatService;
            _logService = logService;
        }

        // Entrar a un grupo - IdChat
        public async Task ConectarAlChat(string idChat)
        {
            try
            {
                var idEmisor = Context.UserIdentifier;
                await Groups.AddToGroupAsync(Context.ConnectionId, idChat);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(ConectarAlChat)}, de la clase {nameof(ChatHub)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
        }

        // Enviar Mensaje al grupo
        public async Task EnviarMensaje(string idChat, string mensaje, string idEmisor)
        {
            try
            {
                //var idEmisor = Context.UserIdentifier;
                //if (string.IsNullOrEmpty(idEmisor))
                //{
                //    throw new HubException("El usuario emisor no está autenticado.");
                //}

                int parsedIdChat = int.Parse(idChat.ToString());
                ChatDTO chat = await _chatService.GetById(parsedIdChat);

                if (chat == null)
                {
                    throw new HubException("No se pudo obtener el chat.");
                }

                var idReceptor = chat.IdUsuario1 != idEmisor ? chat.IdUsuario1 : chat.IdUsuario2;

                await _mensajeService.GuardarMensaje(parsedIdChat, idEmisor, idReceptor, mensaje);

                await Clients.Group(idChat).RecibirMensaje(mensaje, idEmisor);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(EnviarMensaje)}, de la clase {nameof(ChatHub)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                Console.WriteLine($"Error en EnviarMensaje: {ex.Message}");
                throw;
            }
        }

        // Desconectarse del websocket
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

    }

}
