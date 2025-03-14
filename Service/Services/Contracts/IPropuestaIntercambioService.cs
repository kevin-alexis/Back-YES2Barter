using Domain.DTOs;
using Domain.Entities;
using Domain.Enumerations;
using Domain.ViewModels.CreatePropuestaIntercambio;
using Domain.ViewModels.EditPropuestaIntercambio;
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
        Task<EndpointResponse<string>> AddPropuesta(CreatePropuestaIntercambioVM createPropuestaIntercambioVM);
        Task<EndpointResponse<string>> ChangeStatus(int IdPropuestaIntercambio, Enums.EstatusPropuestaIntercambio estatus);
        Task<EndpointResponse<string>> DeletePropuesta(int id);
        Task<EndpointResponse<List<PropuestasIntercambiosVM>>> GetAllByIdObjeto(int idObjeto);
        Task<EndpointResponse<List<PropuestasIntercambiosVM>>> GetAllPropuestas();
        Task<EndpointResponse<string>> UpdatePropuesta(int id, EditPropuestaIntercambioVM editPropuestaIntercambioVM);
    }
}
