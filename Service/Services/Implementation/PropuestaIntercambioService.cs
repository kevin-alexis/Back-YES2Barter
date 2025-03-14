using AutoMapper;
using Azure;
using Dapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.CreatePropuestaIntercambio;
using Domain.ViewModels.EditPropuestaIntercambio;
using Domain.ViewModels.GetChats;
using Domain.ViewModels.GetObjetos;
using Domain.ViewModels.GetPropuestasIntercambios;
using Domain.ViewModels.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using Repository.Context;
using Service.Logging;
using Service.Services.Contracts;
using Service.SignalR;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static Domain.Enumerations.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public async Task<EndpointResponse<string>> AddPropuesta(CreatePropuestaIntercambioVM createPropuestaIntercambioVM)
        {
            try
            {
                var objetoOfertado = _context.Objetos.FirstOrDefault(x=> x.Id == createPropuestaIntercambioVM.IdObjetoOfertado);
                var objetoSolicitado = _context.Objetos.FirstOrDefault(x=> x.Id == createPropuestaIntercambioVM.IdObjetoSolicitado);

                if(objetoOfertado == null || objetoSolicitado == null)
                {
                    return new EndpointResponse<string> { Message = "No se pudo obtener los objetos.", Success = false };
                }

                // Si hay una igual pero diferente estatus, tomar en cuenta que no deberia dejar crearlo
                var propuestaExistente = _context.PropuestasIntercambios.FirstOrDefault(x =>
                x.IdObjetoOfertado == createPropuestaIntercambioVM.IdObjetoOfertado &&
                x.IdObjetoSolicitado == createPropuestaIntercambioVM.IdObjetoSolicitado &&
                !x.EsBorrado
                );

                if(propuestaExistente != null)
                {
                    return new EndpointResponse<string> { Message = "Ya hay una propuesta existente con esos valores.", Success = false };
                }

                var propuestaExistenteInversa = _context.PropuestasIntercambios.FirstOrDefault(x =>
                x.IdObjetoOfertado == createPropuestaIntercambioVM.IdObjetoSolicitado &&
                x.IdObjetoSolicitado == createPropuestaIntercambioVM.IdObjetoOfertado && 
                !x.EsBorrado
                );
                // En caso de que se le este ofreciendo el mismo producto que otro ya ofrecio, checamos el estatus y si esta en enviado,
                // podemos activarle su chat y cambiarle el estatus. ESTO IGUAL SOLO SI VIENE CON ESTATUS ENVIADO
                if (propuestaExistenteInversa != null && propuestaExistenteInversa.Estado == EstatusPropuestaIntercambio.ENVIADA 
                    && (createPropuestaIntercambioVM.Estado == EstatusPropuestaIntercambio.ENVIADA || 
                    createPropuestaIntercambioVM.Estado == EstatusPropuestaIntercambio.ACEPTADA))
                {
                    propuestaExistenteInversa.Estado = EstatusPropuestaIntercambio.ACEPTADA;
                    _dbSet.Update(propuestaExistenteInversa);
                    await _context.SaveChangesAsync();

                    var chatNuevo = new Chat()
                    {
                        IdUsuario1 = propuestaExistenteInversa.IdUsuarioOfertante,
                        IdUsuario2 = propuestaExistenteInversa.IdUsuarioReceptor,
                        IdPropuestaIntercambio = propuestaExistenteInversa.Id,
                        EsBorrado = false,
                    };

                    await _context.Chats.AddAsync(chatNuevo);
                    await _context.SaveChangesAsync();
                    return new EndpointResponse<string> { Message = "Ya existia una propuesta, por lo que fue aceptada y se ha abierto un chat para su seguimiento.", Success = true };
                } else if(propuestaExistenteInversa != null)
                {
                    // En caso de que si exista, pero ya este en otro estatus, se avisa que ya hay una propuesta existente con esos valores
                    return new EndpointResponse<string> { Message = "Ya hay una propuesta existente con esos valores.", Success = false };
                }

                PropuestaIntercambioDTO propuestaIntercambioDTO = new PropuestaIntercambioDTO()
                {
                    IdUsuarioOfertante = objetoOfertado.IdUsuario,
                    IdUsuarioReceptor = objetoSolicitado.IdUsuario,
                    IdObjetoOfertado = createPropuestaIntercambioVM.IdObjetoOfertado,
                    IdObjetoSolicitado = createPropuestaIntercambioVM.IdObjetoSolicitado,
                    Estado = createPropuestaIntercambioVM.Estado,
                    FechaPropuesta = DateTime.Now,
                    EsBorrado = false
                };

                var item = _mapper.Map<PropuestaIntercambio>(propuestaIntercambioDTO);
                await _dbSet.AddAsync(item);
                await _context.SaveChangesAsync();


                var chat = await _context.Chats.FirstOrDefaultAsync(x => x.IdPropuestaIntercambio == item.Id && !x.EsBorrado);

                var estatusDisponibles = new[]{
                    EstatusPropuestaIntercambio.ENVIADA,
                    EstatusPropuestaIntercambio.ACEPTADA,
                    EstatusPropuestaIntercambio.RECHAZADA,
                    EstatusPropuestaIntercambio.NO_CONCRETADA
                };

                if (estatusDisponibles.Contains(item.Estado))
                {
                    await _objetoService.ChangeStatus(item.IdObjetoOfertado, EstatusObjeto.DISPONIBLE);
                    await _objetoService.ChangeStatus(item.IdObjetoSolicitado, EstatusObjeto.DISPONIBLE);
                    if (chat != null)
                    {
                        _context.Chats.Remove(chat);
                    }
                }
                else if (item.Estado == EstatusPropuestaIntercambio.CONCRETADA)
                {
                    await _objetoService.ChangeStatus(item.IdObjetoOfertado, EstatusObjeto.NO_DISPONIBLE);
                    await _objetoService.ChangeStatus(item.IdObjetoSolicitado, EstatusObjeto.NO_DISPONIBLE);

                    if (chat != null)
                    {
                        _context.Chats.Remove(chat);
                    }
                }

                if(item.Estado == EstatusPropuestaIntercambio.ACEPTADA)
                {
                    var chatNuevo = new Chat()
                    {
                        IdUsuario1 = item.IdUsuarioOfertante,
                        IdUsuario2 = item.IdUsuarioReceptor,
                        IdPropuestaIntercambio = item.Id,
                        EsBorrado = false,
                    };

                    await _context.Chats.AddAsync(chatNuevo);
                    await _context.SaveChangesAsync();

                }

                return new EndpointResponse<string> { Message = "Propuesta Creada con Exito.", Success = true };

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

        public async Task<EndpointResponse<string>> UpdatePropuesta(int id, EditPropuestaIntercambioVM editPropuestaIntercambioVM)
        {
            try
            {
                var objetoOfertado = _context.Objetos.FirstOrDefault(x => x.Id == editPropuestaIntercambioVM.IdObjetoOfertado);
                var objetoSolicitado = _context.Objetos.FirstOrDefault(x => x.Id == editPropuestaIntercambioVM.IdObjetoSolicitado);

                if (objetoOfertado == null || objetoSolicitado == null)
                {
                    return new EndpointResponse<string> { Message = "No se pudo obtener los objetos.", Success = false };

                }

                // Si hay una igual pero diferente estatus, tomar en cuenta que no deberia dejar crearlo
                var propuestaExistente = _context.PropuestasIntercambios.FirstOrDefault(x =>
                x.Id != id &&
                x.IdObjetoOfertado == editPropuestaIntercambioVM.IdObjetoOfertado &&
                x.IdObjetoSolicitado == editPropuestaIntercambioVM.IdObjetoSolicitado &&
                !x.EsBorrado
                );

                if (propuestaExistente != null)
                {
                    return new EndpointResponse<string> { Message = "Ya hay una propuesta existente con esos valores.", Success = false };
                }

                var propuestaExistenteInversa = _context.PropuestasIntercambios.FirstOrDefault(x =>
                x.Id != id &&
                x.IdObjetoOfertado == editPropuestaIntercambioVM.IdObjetoSolicitado &&
                x.IdObjetoSolicitado == editPropuestaIntercambioVM.IdObjetoOfertado &&
                !x.EsBorrado
                );
                // En caso de que se le este ofreciendo el mismo producto que otro ya ofrecio, checamos el estatus y si esta en enviado,
                // podemos activarle su chat y cambiarle el estatus. ESTO IGUAL SOLO SI VIENE CON ESTATUS ENVIADO
                if (propuestaExistenteInversa != null && propuestaExistenteInversa.Estado == EstatusPropuestaIntercambio.ENVIADA
                    && (editPropuestaIntercambioVM.Estado == EstatusPropuestaIntercambio.ENVIADA ||
                    editPropuestaIntercambioVM.Estado == EstatusPropuestaIntercambio.ACEPTADA))
                {
                    // Mandamos a llamar la funcion DeletePropuesta pues debe tener otra logica mas que solo el delete de EF
                    await DeletePropuesta(propuestaExistenteInversa.Id);
                }
                else if (propuestaExistenteInversa != null)
                {
                    // En caso de que si exista, pero ya este en otro estatus, se avisa que ya hay una propuesta existente con esos valores
                    return new EndpointResponse<string> { Message = "Ya hay una propuesta con esos valores en proceso.", Success = false };
                }

                var propuestaIntercambio = await _context.PropuestasIntercambios.FirstOrDefaultAsync(x => x.Id == id);

                propuestaIntercambio.IdUsuarioOfertante = objetoOfertado.IdUsuario;
                propuestaIntercambio.IdUsuarioReceptor = objetoSolicitado.IdUsuario;
                propuestaIntercambio.IdObjetoOfertado = editPropuestaIntercambioVM.IdObjetoOfertado;
                propuestaIntercambio.IdObjetoSolicitado = editPropuestaIntercambioVM.IdObjetoSolicitado;
                propuestaIntercambio.Estado = editPropuestaIntercambioVM.Estado;

                _dbSet.Update(propuestaIntercambio);
                await _context.SaveChangesAsync();


                var chat = await _context.Chats.FirstOrDefaultAsync(x => x.IdPropuestaIntercambio == propuestaIntercambio.Id && !x.EsBorrado);

                var estatusDisponibles = new[]{
                    EstatusPropuestaIntercambio.ENVIADA,
                    EstatusPropuestaIntercambio.ACEPTADA,
                    EstatusPropuestaIntercambio.RECHAZADA,
                    EstatusPropuestaIntercambio.NO_CONCRETADA
                };

                if (estatusDisponibles.Contains(propuestaIntercambio.Estado))
                {
                    await _objetoService.ChangeStatus(propuestaIntercambio.IdObjetoOfertado, EstatusObjeto.DISPONIBLE);
                    await _objetoService.ChangeStatus(propuestaIntercambio.IdObjetoSolicitado, EstatusObjeto.DISPONIBLE);
                    if (chat != null)
                    {
                        _context.Chats.Remove(chat);
                    }
                }
                else if (propuestaIntercambio.Estado == EstatusPropuestaIntercambio.CONCRETADA)
                {
                    await _objetoService.ChangeStatus(propuestaIntercambio.IdObjetoOfertado, EstatusObjeto.NO_DISPONIBLE);
                    await _objetoService.ChangeStatus(propuestaIntercambio.IdObjetoSolicitado, EstatusObjeto.NO_DISPONIBLE);

                    if (chat != null)
                    {
                        _context.Chats.Remove(chat);
                    }
                }

                if (propuestaIntercambio.Estado == EstatusPropuestaIntercambio.ACEPTADA)
                {
                    var chatNuevo = new Chat()
                    {
                        IdUsuario1 = propuestaIntercambio.IdUsuarioOfertante,
                        IdUsuario2 = propuestaIntercambio.IdUsuarioReceptor,
                        IdPropuestaIntercambio = propuestaIntercambio.Id,
                        EsBorrado = false,
                    };

                    await _context.Chats.AddAsync(chatNuevo);
                    await _context.SaveChangesAsync();

                }
                return new EndpointResponse<string> { Message = "Propuesta Editada con Exito.", Success = true };

            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(UpdatePropuesta)}, de la clase {nameof(PropuestaIntercambioService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                }); throw new Exception("Error al agregar el elemento", ex);
            }
        }

        public async Task<EndpointResponse<string>> DeletePropuesta(int id)
        {
            try
            {
                PropuestaIntercambio propuestaIntercambio = await _context.PropuestasIntercambios.FirstOrDefaultAsync(x => x.Id == id && !x.EsBorrado);

                // si la propuesta esta activa, eliminar su chat
                if(propuestaIntercambio != null && propuestaIntercambio.Estado == EstatusPropuestaIntercambio.ACEPTADA)
                {
                    var chat = await _context.Chats.FirstOrDefaultAsync(x => x.IdPropuestaIntercambio == propuestaIntercambio.Id && !x.EsBorrado);
                    if (chat != null)
                    {
                        _context.Chats.Remove(chat);
                    }
                }
                
                _context.Remove(propuestaIntercambio);
                await _context.SaveChangesAsync();

                return new EndpointResponse<string> { Message = "Propuesta Eliminada con Exito.", Success = true };
            }
            catch (Exception ex)
            {

                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(DeletePropuesta)}, de la clase {nameof(PropuestaIntercambioService)}: {ex.Message}",
                    Excepcion = ex.ToString()
                }); 
                throw new Exception("Error al eliminar el elemento", ex);
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


        public async Task<EndpointResponse<List<PropuestasIntercambiosVM>>> GetAllByIdObjeto(int idObjeto)
        {
            try
            {
                if (idObjeto <= 0)
                {
                    return new EndpointResponse<List<PropuestasIntercambiosVM>>
                    {
                        Message = "El id es requerido",
                        Success = false,
                        Data = new List<PropuestasIntercambiosVM>()
                    };
                }

                var result = await _context.PropuestasIntercambios
                    .Where(x => x.EsBorrado == false && (x.IdObjetoOfertado == idObjeto || x.IdObjetoSolicitado == idObjeto))
                    .ToListAsync();

                if (!result.Any())
                {
                    return new EndpointResponse<List<PropuestasIntercambiosVM>>
                    {
                        Message = "No se encontraron propuestas de intercambios con ese objeto",
                        Success = false,
                        Data = new List<PropuestasIntercambiosVM>()
                    };
                }

                var propuestaIntercambioDTOs = await Task.WhenAll(result.Select(async propuestaIntercambio =>
                {
                    var personaOfertante = await _context.Personas.FirstOrDefaultAsync(p => p.IdUsuario == propuestaIntercambio.IdUsuarioOfertante);
                    var personaReceptor = await _context.Personas.FirstOrDefaultAsync(p => p.IdUsuario == propuestaIntercambio.IdUsuarioReceptor);
                    var objetoOfertado = await _context.Objetos.FirstOrDefaultAsync(p => p.Id == propuestaIntercambio.IdObjetoOfertado);
                    var objetoSolicitado = await _context.Objetos.FirstOrDefaultAsync(p => p.Id == propuestaIntercambio.IdObjetoSolicitado);

                    return new PropuestasIntercambiosVM
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
                    };
                }));

                return new EndpointResponse<List<PropuestasIntercambiosVM>>
                {
                    Message = "Propuestas de intercambios obtenidas con éxito",
                    Success = true,
                    Data = propuestaIntercambioDTOs.ToList()
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

        public async Task<EndpointResponse<string>> ChangeStatus(int IdPropuestaIntercambio, EstatusPropuestaIntercambio estatus)
        {
            try
            {
                var propuestaIntercambio = await _context.PropuestasIntercambios.FirstOrDefaultAsync(x => x.Id == IdPropuestaIntercambio);

                if (propuestaIntercambio == null)
                {
                    return new EndpointResponse<string>
                    {
                        Message = "No se pudo obtener la propuesta de intercambio.",
                        Success = false,
                    };
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
                return new EndpointResponse<string>
                {
                    Message = "Estado de propuesta cambiado con exito.",
                    Success = true,
                };
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
