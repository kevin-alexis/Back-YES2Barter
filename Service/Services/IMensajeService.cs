using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs;

namespace Service.Services
{
    public interface IMensajeService
    {
        Task<IEnumerable<MensajeDTO>> ObtenerMensajesAsync(int emisorId, int receptorId);
        Task EnviarMensajeAsync(MensajeDTO mensaje);
    }
}
