using AutoMapper;
using Azure;
using Dapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.CreatePropuestaIntercambio;
using Domain.ViewModels.GetChats;
using Domain.ViewModels.GetPropuestasIntercambios;
using Domain.ViewModels.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repository.Context;
using Service.Logging;
using Service.Services.Contracts;
using Service.SignalR;
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
        private readonly ILogService _logService;
        public PropuestaIntercambioService(
            DataBaseContext context, 
            IMapper mapper, Logger logger, 
            Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager, 
            IObjetoService objetoService,
            ILogService logService
            ) : base(context, mapper, logService)
        {
            _userManager = userManager;
            _objetoService = objetoService;
            _logService = logService;
        }

        virtual public async Task AddPropuesta(CreatePropuestaIntercambioVM createPropuestaIntercambioVM)
        {
            try
            {
                var objetoOfertado = _context.Objetos.FirstOrDefault(x=> x.Id == createPropuestaIntercambioVM.IdObjetoOfertado);
                var objetoSolicitado = _context.Objetos.FirstOrDefault(x=> x.Id == createPropuestaIntercambioVM.IdObjetoSolicitado);

                if(objetoOfertado == null || objetoSolicitado == null)
                {
                    throw new HubException("No se pudo obtener los objetos.");
                }

                createPropuestaIntercambioVM.IdUsuarioOfertante = objetoOfertado.IdUsuario;
                createPropuestaIntercambioVM.IdUsuarioOfertante = objetoSolicitado.IdUsuario;
                createPropuestaIntercambioVM.Estado = EstatusPropuestaIntercambio.ENVIADA;
                var item = _mapper.Map<PropuestaIntercambio>(createPropuestaIntercambioVM);
                await _dbSet.AddAsync(item);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(AddPropuesta)}, de la clase {nameof(PropuestaIntercambioService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                }); throw new Exception("Error al agregar el elemento", ex);
            }
        }

        public async Task<EndpointResponse<List<PropuestasIntercambiosVM>>> GetAllPropuestas()
        {
            try
            {
                var result = await _context.PropuestasIntercambios
                    .Where(x => !x.EsBorrado)
                    .ToListAsync();

                if (!result.Any())
                {
                    return new EndpointResponse<List<PropuestasIntercambiosVM>>
                    {
                        Message = "No se encontraron propuestas de intercambio",
                        Success = false,
                        Data = new List<PropuestasIntercambiosVM>()
                    };
                }

                var propuestaIntercambioDTOs = new List<PropuestasIntercambiosVM>();

                foreach (var propuestaIntercambio in result)
                {
                    var personaOfertante = await _context.Personas.FirstOrDefaultAsync(p => p.IdUsuario == propuestaIntercambio.IdUsuarioOfertante);
                    var personaReceptor = await _context.Personas.FirstOrDefaultAsync(p => p.IdUsuario == propuestaIntercambio.IdUsuarioReceptor);
                    var objetoOfertado = await _context.Objetos.FirstOrDefaultAsync(p => p.Id == propuestaIntercambio.IdObjetoOfertado);
                    var objetoSolicitado = await _context.Objetos.FirstOrDefaultAsync(p => p.Id == propuestaIntercambio.IdObjetoSolicitado);

                    propuestaIntercambioDTOs.Add(new PropuestasIntercambiosVM
                    {
                        Id = propuestaIntercambio.Id,
                        IdUsuarioOfertante = propuestaIntercambio.IdUsuarioOfertante,
                        PersonaOfertante = personaOfertante,
                        IdUsuarioReceptor = propuestaIntercambio.IdUsuarioReceptor,
                        PersonaReceptor = personaReceptor,
                        IdObjetoOfertado = propuestaIntercambio.IdObjetoOfertado,
                        ObjetoOfertado = objetoOfertado,
                        IdObjetoSolicitado = propuestaIntercambio.IdObjetoSolicitado, 
                        ObjetoSolicitado = objetoSolicitado,
                        FechaPropuesta = propuestaIntercambio.FechaPropuesta,
                        Estado = propuestaIntercambio.Estado
                    });
                }

                return new EndpointResponse<List<PropuestasIntercambiosVM>>
                {
                    Message = "Propuestas de intercambio obtenidos con éxito",
                    Success = true,
                    Data = propuestaIntercambioDTOs
                };
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetAllPropuestas)}, de la clase {nameof(PropuestaIntercambioService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
        }


        public async Task<EndpointResponse<List<PropuestaIntercambioDTO>>> GetAllByIdObjeto(int idObjeto)
        {
            try
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
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetAllByIdObjeto)}, de la clase {nameof(PropuestaIntercambioService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
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
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(ChangeStatus)}, de la clase {nameof(PropuestaIntercambioService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw new Exception("Error al actualizar el elemento", ex);
            }
        }

    }
}
