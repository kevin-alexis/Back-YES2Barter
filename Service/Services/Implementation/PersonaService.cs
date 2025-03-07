using Dapper;
using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using Service.Services.Contracts;
using Service.Logging;
using Domain.ViewModels.Response;
using System.IdentityModel.Tokens.Jwt;
using static Domain.Enumerations.Enums;

namespace Service.Services.Implementation
{
    public class PersonaService : BaseService<Persona, PersonaDTO>, IPersonaService
    {
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        public PersonaService(Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager,
           DataBaseContext context, IMapper mapper, Logger logger) : base(context, mapper, logger)
        {
            _userManager = userManager;
        }

        // Obtener Persona por Id usando Entity Framework
        public async Task<PersonaDTO> GetPersonaByIdEf(int id)
        {
            try
            {
                var item = await _dbSet.Where(e => e.Id == id && !e.EsBorrado).FirstOrDefaultAsync();
                if (item != null)
                {
                    return _mapper.Map<PersonaDTO>(item);
                }
                return default(PersonaDTO);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener el elemento con id {id}", ex);
            }
        }

        // Obtener Persona por Id usando Dapper
        public async Task<PersonaDTO> GetPersonaByIdDapper(int id)
        {
            try
            {
                using (var connection = _context.Database.GetDbConnection())
                {
                    var query = "SELECT * FROM Tbl_Personas WHERE Id = @Id AND EsBorrado = 0";
                    var item = await connection.QueryFirstOrDefaultAsync<Persona>(query, new { Id = id });
                    if (item != null)
                    {
                        return _mapper.Map<PersonaDTO>(item);
                    }
                    return default(PersonaDTO);
                }
            }

            catch (Exception ex)
            {
                throw new Exception($"Error al obtener el elemento con id {id}", ex);
            }
        }

        public async Task<EndpointResponse<List<PersonaDTO>>> GetAllByRol(string token)
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

            var result = new List<PersonaDTO>();
            switch (role)
            {
                case "Admin":
                    string sqlAdmin = @"SELECT DISTINCT Tbl_Personas.* FROM Tbl_Personas 
                                        INNER JOIN AspNetUsers on AspNetUsers.Id = Tbl_Personas.IdUsuario
                                        INNER JOIN AspNetUserRoles on AspNetUsers.Id = AspNetUserRoles.UserId
                                        INNER JOIN AspNetRoles on AspNetRoles.Id = AspNetUserRoles.RoleId
                                        WHERE Tbl_Personas.EsBorrado = @EsBorrado
                                        AND Tbl_Personas.EsBorrado = @EsBorrado
                                        AND AspNetRoles.Name = @Rol";

                    using (var connection = _context.Database.GetDbConnection())
                    {
                        await connection.OpenAsync();
                        result = (await connection.QueryAsync<PersonaDTO>(sqlAdmin, new { EsBorrado = false, Rol = "Inquilino" })).ToList();
                    }
                    break;

                case "Dueño" or "Encargado":
                    var ownerId = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                    var user = await _userManager.FindByIdAsync(ownerId);

                    if (string.IsNullOrEmpty(ownerId))
                    {
                        throw new Exception("El token no contiene un identificador de usuario válido para el Dueño.");
                    }

                    string sql = @"SELECT DISTINCT Tbl_Personas.* FROM Tbl_Personas 
                                    INNER JOIN AspNetUsers on AspNetUsers.Id = Tbl_Personas.IdUsuario
                                    INNER JOIN AspNetUserRoles on AspNetUsers.Id = AspNetUserRoles.UserId
                                    INNER JOIN AspNetRoles on AspNetRoles.Id = AspNetUserRoles.RoleId
                                    WHERE Tbl_Personas.EsBorrado = @EsBorrado
                                    AND Tbl_Personas.EsBorrado = @EsBorrado
                                    AND AspNetRoles.Name = @Rol
                                    AND AspNetUsers.IdEdificio = @IdEdificio";
                    using (var connection = _context.Database.GetDbConnection())
                    {
                        await connection.OpenAsync();
                        result = (await connection.QueryAsync<PersonaDTO>(sql, new { EsBorrado = false, Rol = "Inquilino" })).ToList();
                    }
                    break;
                default:
                    throw new Exception("El Rol especificado no tiene acceso a las personas.");
            }

            if (!result.Any())
            {
                return new EndpointResponse<List<PersonaDTO>> { Message = "No hay personas registradas", Success = true, Data = [] };
            }
            return new EndpointResponse<List<PersonaDTO>> { Message = "Personas obtenidas con exito", Success = true, Data = result };
        }
    }
}
