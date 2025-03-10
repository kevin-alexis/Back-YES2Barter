using AutoMapper;
using Azure;
using Dapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.GetPropuestasIntercambios;
using Domain.ViewModels.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repository.Context;
using Service.Logging;
using Service.Services.Contracts;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Enumerations.Enums;

namespace Service.Services.Implementation
{
    public class PropuestaIntercambioService : BaseService<PropuestaIntercambio, PropuestaIntercambioDTO>, IPropuestaIntercambioService
    {
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        private readonly IObjetoService _objetoService;
        public PropuestaIntercambioService(
            DataBaseContext context, IMapper mapper, Logger logger, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager, IObjetoService objetoService) : base(context, mapper, logger)
        {
            _userManager = userManager;
            _objetoService = objetoService;
        }

        public async Task<EndpointResponse<List<PropuestaIntercambioDTO>>> GetAllByIdObjeto(int idObjeto)
        {
            if (idObjeto <= 0)
            {
                return new EndpointResponse<List<PropuestaIntercambioDTO>>
                {
                    Message = "El id es requerido",
                    Success = false,
                    Data = new List<PropuestaIntercambioDTO>()
                };
            }

            var result = await _context.PropuestasIntercambios
                .Where(x => x.EsBorrado == false)
                .ToListAsync();

            if (!result.Any())
            {
                return new EndpointResponse<List<PropuestaIntercambioDTO>>
                {
                    Message = "No se encontraron propuestas de intercambios con ese objeto",
                    Success = false,
                    Data = new List<PropuestaIntercambioDTO>()
                };
            }

            var propuestaIntercambioDTOs = result.Select(propuestaIntercambio => new PropuestaIntercambioDTO
            {
                Id = propuestaIntercambio.Id,
                EsBorrado = propuestaIntercambio.EsBorrado
            }).ToList();

            return new EndpointResponse<List<PropuestaIntercambioDTO>>
            {
                Message = "Propuestas de intercambios obtenidas con éxito",
                Success = true,
                Data = propuestaIntercambioDTOs
            };
        }

        public async Task ChangeStatus(int IdPropuestaIntercambio, EstatusPropuestaIntercambio estatus)
        {
            try
            {
                var propuestaIntercambio = await _context.PropuestasIntercambios.FirstOrDefaultAsync(x => x.Id == IdPropuestaIntercambio);

                if (propuestaIntercambio == null)
                {
                    throw new HubException("No se pudo obtener la propuesta de intercambio.");
                }

                propuestaIntercambio.Estado = estatus;

                var estatusDisponibles = new[]{
                    EstatusPropuestaIntercambio.ENVIADA,
                    EstatusPropuestaIntercambio.ACEPTADA,
                    EstatusPropuestaIntercambio.RECHAZADA,
                    EstatusPropuestaIntercambio.NO_CONCRETADA
                };

                if (estatusDisponibles.Contains(estatus))
                {
                    await _objetoService.ChangeStatus(propuestaIntercambio.IdObjetoOfertado, EstatusObjeto.DISPONIBLE);
                    await _objetoService.ChangeStatus(propuestaIntercambio.IdObjetoSolicitado, EstatusObjeto.DISPONIBLE);
                }
                else if (estatus == EstatusPropuestaIntercambio.CONCRETADA)
                {
                    await _objetoService.ChangeStatus(propuestaIntercambio.IdObjetoOfertado, EstatusObjeto.NO_DISPONIBLE);
                    await _objetoService.ChangeStatus(propuestaIntercambio.IdObjetoSolicitado, EstatusObjeto.NO_DISPONIBLE);
                }


                _dbSet.Update(propuestaIntercambio);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //await _logger.LogAsync("Error", "Error al actualizar el elemento", ex.ToString());
                throw new Exception("Error al actualizar el elemento", ex);
            }
        }

    }
}
