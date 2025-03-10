using Domain.DTOs;
using Domain.Entities;
using Domain.Enumerations;
using Domain.ViewModels.Response;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Contracts
{
    public interface IPropuestaIntercambioService : IBaseService<PropuestaIntercambio, PropuestaIntercambioDTO>
    {
        Task ChangeStatus(int IdPropuestaIntercambio, Enums.EstatusPropuestaIntercambio estatus);
        Task<EndpointResponse<List<PropuestaIntercambioDTO>>> GetAllByIdObjeto(int idObjeto);
    }
}
