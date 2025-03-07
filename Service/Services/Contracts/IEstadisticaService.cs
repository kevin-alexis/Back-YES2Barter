using Domain.ViewModels.GetEstadisticas;
using Domain.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Contracts
{
    public interface IEstadisticaService
    {
        Task<EndpointResponse<EstadisticasVM>> GetEstadisticasByUser(string token);
    }
}
