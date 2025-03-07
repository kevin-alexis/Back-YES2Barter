using AutoMapper;
using Azure;
using Dapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.Response;
using Microsoft.AspNetCore.Http;
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
        private new readonly DataBaseContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        public PropuestaIntercambioService(
            DataBaseContext context, IMapper mapper, Logger logger, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager) : base(context, mapper, logger)
        {
            _context = context;
            _userManager = userManager;
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

        virtual public async Task<PropuestaIntercambioDTO> GetById(int id)
        {
            try
            {
                var item = await _dbSet.Where(e => e.Id == id && !EF.Property<bool>(e, "EsBorrado")).FirstOrDefaultAsync();
                if (item != null)
                {
                    return _mapper.Map<PropuestaIntercambioDTO>(item);
                }
                return default(PropuestaIntercambioDTO);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener el elemento con id {id}", ex);
            }
        }
    }
}
