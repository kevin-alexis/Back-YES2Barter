using AutoMapper;
using Dapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.GetEstadisticas;
using Domain.ViewModels.Response;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
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
    public class EstadisticaService : IEstadisticaService
    {
        private readonly DataBaseContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        public EstadisticaService(DataBaseContext context, IMapper mapper, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager) 
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<EndpointResponse<EstadisticasVM>> GetEstadisticasByUser(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
            {
                throw new Exception("El token proporcionado no es válido.");
            }

            var jwtToken = handler.ReadJwtToken(token);

            var role = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

            if (string.IsNullOrEmpty(role))
            {
                throw new Exception("El token no contiene un Rol válido.");
            }

            var result = new EstadisticasVM();
            switch (role)
            {
                case "Admin":
                    string sqlAdmin = @" SELECT
                                        (SELECT COUNT(*)
                                            FROM Tbl_Personas
                                            INNER JOIN AspNetUsers ON Tbl_Personas.IdUsuario = AspNetUsers.Id
                                            INNER JOIN AspNetUserRoles ON AspNetUserRoles.UserId = AspNetUsers.Id
                                            INNER JOIN AspNetRoles ON AspNetUserRoles.RoleId = AspNetRoles.Id
                                            INNER JOIN Tbl_Edificios ON AspNetUsers.IdEdificio = Tbl_Edificios.Id
                                            WHERE Tbl_Personas.EsBorrado = @EsBorrado) AS totalDeUsuarios,

                                        (SELECT COUNT(*)
                                            FROM Tbl_Edificios
                                            WHERE EsBorrado = @EsBorrado) AS totalDeEdificios,

                                        (SELECT COUNT(*)
                                            FROM Tbl_Habitaciones
                                            WHERE EsBorrado = @EsBorrado) AS totalDeHabitaciones;
                                        ";

                    using (var connection = _context.Database.GetDbConnection())
                    {
                        await connection.OpenAsync();

                        var estadisticas = await connection.QueryFirstOrDefaultAsync<EstadisticasVM>(
                            sqlAdmin,
                            new { EsBorrado = false }
                        );

                        result = estadisticas ?? new EstadisticasVM();
                    }
                    break;

                case "Dueño":
                    var ownerId = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                    if (string.IsNullOrEmpty(ownerId))
                    {
                        throw new Exception("El token no contiene un identificador de usuario válido para el Dueño.");
                    }

                    var user = await _userManager.FindByIdAsync(ownerId);
                    if (user == null)
                    {
                        throw new Exception("El usuario no está asociado a un edificio válido.");
                    }

                    string sqlDueño = @"SELECT 
                                        (SELECT COUNT(*)
                                         FROM Tbl_Personas
                                         INNER JOIN AspNetUsers ON Tbl_Personas.IdUsuario = AspNetUsers.Id
                                         INNER JOIN AspNetUserRoles ON AspNetUserRoles.UserId = AspNetUsers.Id
                                         INNER JOIN AspNetRoles ON AspNetUserRoles.RoleId = AspNetRoles.Id
                                         INNER JOIN Tbl_Edificios ON AspNetUsers.IdEdificio = Tbl_Edificios.Id
                                         WHERE Tbl_Personas.EsBorrado = @EsBorrado 
                                           AND AspNetUsers.IdEdificio = @IdEdificio
                                           AND AspNetRoles.Name IN ('Encargado')) AS totalDeEncargados,

                                        (SELECT COUNT(*)
                                         FROM Tbl_Personas
                                         INNER JOIN AspNetUsers ON Tbl_Personas.IdUsuario = AspNetUsers.Id
                                         INNER JOIN AspNetUserRoles ON AspNetUserRoles.UserId = AspNetUsers.Id
                                         INNER JOIN AspNetRoles ON AspNetUserRoles.RoleId = AspNetRoles.Id
                                         INNER JOIN Tbl_Edificios ON AspNetUsers.IdEdificio = Tbl_Edificios.Id
                                         WHERE Tbl_Personas.EsBorrado = @EsBorrado 
                                           AND AspNetUsers.IdEdificio = @IdEdificio
                                           AND AspNetRoles.Name IN ('Inquilino')) AS totalDeInquilinos,

                                        (SELECT COUNT(*)
                                         FROM Tbl_Contratos 
                                         INNER JOIN Tbl_Habitaciones ON Tbl_Contratos.IdHabitacion = Tbl_Habitaciones.Id
                                         WHERE Tbl_Contratos.EsBorrado = @EsBorrado 
                                           AND Tbl_Habitaciones.EsBorrado = @EsBorrado 
                                           AND Tbl_Habitaciones.IdEdificio = @IdEdificio
                                           AND Tbl_Contratos.EstatusContrato = @EstatusContrato) AS totalDeContratosActivos,

                                        (SELECT SUM(Tbl_Pagos.Monto)
                                         FROM Tbl_Pagos
                                         INNER JOIN Tbl_Contratos ON Tbl_Contratos.Id = Tbl_Pagos.IdContrato
                                         INNER JOIN Tbl_Habitaciones ON Tbl_Habitaciones.Id = Tbl_Contratos.IdHabitacion
                                         WHERE Tbl_Pagos.EsBorrado = @EsBorrado
                                           AND Tbl_Contratos.EsBorrado = @EsBorrado
                                           AND Tbl_Habitaciones.EsBorrado = @EsBorrado
                                           AND Tbl_Habitaciones.IdEdificio = @IdEdificio) AS totalDeIngresos,

                                        (SELECT SUM(Tbl_Mantenimientos.Costo)
                                         FROM Tbl_Mantenimientos
                                         INNER JOIN Tbl_Contratos ON Tbl_Mantenimientos.IdContrato = Tbl_Contratos.Id
                                         INNER JOIN Tbl_Habitaciones ON Tbl_Contratos.IdHabitacion = Tbl_Habitaciones.Id
                                         WHERE Tbl_Mantenimientos.EsBorrado = @EsBorrado
                                           AND Tbl_Contratos.EsBorrado = @EsBorrado
                                           AND Tbl_Habitaciones.EsBorrado = @EsBorrado
                                           AND Tbl_Habitaciones.IdEdificio = @IdEdificio) AS totalDeGastos;
                                        ";

                    using (var connection = _context.Database.GetDbConnection())
                    {
                        await connection.OpenAsync();

                        var estadisticas = await connection.QueryFirstOrDefaultAsync<EstadisticasVM>(
                            sqlDueño,
                            new
                            {
                                EsBorrado = false,
                            }
                        );

                        result = estadisticas ?? new EstadisticasVM();
                    }
                    break;



                case "Encargado":
                    var ownerIdEncargado = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                    if (string.IsNullOrEmpty(ownerIdEncargado))
                    {
                        throw new Exception("El token no contiene un identificador de usuario válido para el Dueño.");
                    }

                    var userEncargado = await _userManager.FindByIdAsync(ownerIdEncargado);
                    if (userEncargado == null)
                    {
                        throw new Exception("El usuario no está asociado a un edificio válido.");
                    }

                    string sqlInquilino = @"SELECT 
                                            (SELECT 
                                            COUNT(*)
                                            FROM Tbl_Habitaciones 
                                            WHERE EsBorrado = @EsBorrado 
                                            AND EstatusHabitacion = @EstatusHabitacion
                                            AND IdEdificio = @IdEdificio) AS totalDeHabitacionesDisponibles,

                                            (SELECT 
                                            COUNT(*)
                                            FROM Tbl_Mantenimientos 
                                            INNER JOIN Tbl_Contratos on Tbl_Mantenimientos.IdContrato = Tbl_Contratos.Id
                                            INNER JOIN Tbl_Personas on Tbl_Contratos.IdInquilino = Tbl_Personas.Id
                                            INNER JOIN Tbl_Habitaciones on Tbl_Contratos.IdHabitacion = Tbl_Habitaciones.Id
                                            WHERE Tbl_Mantenimientos.EsBorrado = @EsBorrado
                                            AND Tbl_Contratos.EsBorrado = @EsBorrado
                                            AND Tbl_Personas.EsBorrado = @EsBorrado
                                            AND Tbl_Habitaciones.EsBorrado = @EsBorrado
                                            AND Tbl_Habitaciones.IdEdificio = @IdEdificio
                                            AND Tbl_Mantenimientos.Estatus = @EstatusPendientes) AS mantenimientosPendientes,

                                            (SELECT 
                                            COUNT(*) 
                                            FROM Tbl_Mantenimientos 
                                            INNER JOIN Tbl_Contratos on Tbl_Mantenimientos.IdContrato = Tbl_Contratos.Id
                                            INNER JOIN Tbl_Personas on Tbl_Contratos.IdInquilino = Tbl_Personas.Id
                                            INNER JOIN Tbl_Habitaciones on Tbl_Contratos.IdHabitacion = Tbl_Habitaciones.Id
                                            WHERE Tbl_Mantenimientos.EsBorrado = @EsBorrado
                                            AND Tbl_Contratos.EsBorrado = @EsBorrado
                                            AND Tbl_Personas.EsBorrado = @EsBorrado
                                            AND Tbl_Habitaciones.EsBorrado = @EsBorrado
                                            AND Tbl_Habitaciones.IdEdificio = @IdEdificio
                                            AND Tbl_Mantenimientos.Estatus = @EstatusEnProceso) AS mantenimientosEnProceso,

                                            (SELECT 
                                            COUNT(*) 
                                            FROM Tbl_Mantenimientos 
                                            INNER JOIN Tbl_Contratos on Tbl_Mantenimientos.IdContrato = Tbl_Contratos.Id
                                            INNER JOIN Tbl_Personas on Tbl_Contratos.IdInquilino = Tbl_Personas.Id
                                            INNER JOIN Tbl_Habitaciones on Tbl_Contratos.IdHabitacion = Tbl_Habitaciones.Id
                                            WHERE Tbl_Mantenimientos.EsBorrado = @EsBorrado
                                            AND Tbl_Contratos.EsBorrado = @EsBorrado
                                            AND Tbl_Personas.EsBorrado = @EsBorrado
                                            AND Tbl_Habitaciones.EsBorrado = @EsBorrado
                                            AND Tbl_Habitaciones.IdEdificio = @IdEdificio
                                            AND Tbl_Mantenimientos.Estatus = @EstatusFinalizado) AS mantenimientosFinalizados;";
                    using (var connection = _context.Database.GetDbConnection())
                    {
                        await connection.OpenAsync();

                        var estadisticas = await connection.QueryFirstOrDefaultAsync<EstadisticasVM>(
                            sqlInquilino,
                            new
                            {
                                EsBorrado = false,
                            }
                        );

                        result = estadisticas ?? new EstadisticasVM();
                    }
                    break;
                default:
                    throw new Exception("El Rol especificado no tiene acceso a los mantenimientos.");
            }

            if (result == null)
            {
                return new EndpointResponse<EstadisticasVM> { Message = "No hay mantenimientos registrados", Success = true, Data = { } };
            }
            return new EndpointResponse<EstadisticasVM> { Message = "Mantenimientos obtenidos con exito", Success = true, Data = result };
        }
    }
}
