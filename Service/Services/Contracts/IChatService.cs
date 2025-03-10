using Domain.DTOs;
using Domain.Entities;
using Domain.Enumerations;
using Domain.ViewModels.GetChats;
using Domain.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Contracts
{
    public interface IChatService : IBaseService<Chat, ChatDTO>
    {
        Task CloseChat(int idChat, bool isSuccess);
        Task<EndpointResponse<List<GetChatsVM>>> GetAllByIdUsuario(string idUsuario);
    }
}
