using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Contracts
{
    public interface IChatClient
    {

        // apartir de aqui es websocket
        Task RecibirMensaje(string message, string idEmisor);
    }
}
