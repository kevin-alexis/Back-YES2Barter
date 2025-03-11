using Domain.DTOs;
using Domain.Entities;
using Domain.Enumerations;
using Domain.ViewModels.CreatePropuestaIntercambio;
using Domain.ViewModels.GetPropuestasIntercambios;
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
        Task AddPropuesta(CreatePropuestaIntercambioVM createPropuestaIntercambioVM);
        Task ChangeStatus(int IdPropuestaIntercambio, Enums.EstatusPropuestaIntercambio estatus);
        Task<EndpointResponse<List<PropuestaIntercambioDTO>>> GetAllByIdObjeto(int idObjeto);
        Task<EndpointResponse<List<PropuestasIntercambiosVM>>> GetAllPropuestas();
    }
}
