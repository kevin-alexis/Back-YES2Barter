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

        public ChatHub(IMensajeService mensajeService, IChatService chatService)
        {
            _mensajeService = mensajeService;
            _chatService = chatService;
        }

        // Entrar a un grupo - IdChat
        public async Task ConectarAlChat(string idChat)
        {
            var idEmisor = Context.UserIdentifier;
            await Groups.AddToGroupAsync(Context.ConnectionId, idChat);
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
