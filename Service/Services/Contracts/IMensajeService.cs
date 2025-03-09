using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.GetMensajes;
using Domain.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Contracts
{
    public interface IMensajeService : IBaseService<Mensaje, MensajeDTO>
    {
        Task<EndpointResponse<List<GetMensajesVM>>> GetAllByIdChat(int idChat);
        Task GuardarMensaje(int idChat, string idEmisor, string idReceptor, string contenido);

    }
}
