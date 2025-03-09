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
    public interface IObjetoService : IBaseService<Objeto, ObjetoDTO>
    {
        Task ChangeStatus(int IdObjeto, Enums.EstatusObjeto estatus);
        Task<bool> EliminarObjetoImagen(int idObjeto, string ruta);
        Task<EndpointResponse<List<ObjetoDTO>>> GetAllByIdCategoria(int idCategoria);
        Task<EndpointResponse<List<ObjetoDTO>>> GetByName(string name);
        Task<string> GuardarObjetoImagen(int idCategoria, IFormFile objetoImagen, string ruta);
    }
}
