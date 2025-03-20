using AutoMapper;
using Dapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.Account;
using Domain.ViewModels.Response;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using Service.Logging;
using Service.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Domain.Enumerations.Enums;

namespace Service.Services.Implementation
{
    public class CategoriaService : BaseService<Categoria, CategoriaDTO>, ICategoriaService
    {
        private new readonly DataBaseContext _context;
        private readonly ILogService _logService;
        public CategoriaService(DataBaseContext context, IMapper mapper, ILogService logService) : base(context, mapper, logService)
        {
            _context = context;
            _logService = logService;
        }

        
        public override async Task<EndpointResponse<int>> Delete(int id)
        {
            try
            {
                var categoria = await _context.Set<Categoria>().FindAsync(id);
                if (categoria == null)
                {
                    return new EndpointResponse<int>
                    {
                        Success = false,
                        Message = "La categoría no existe.",
                        Data = 0
                    };
                }

               
                bool tieneObjetosAsociados = await _context.Set<Objeto>()
                    .AnyAsync(o => o.IdCategoria == id);

                if (tieneObjetosAsociados)
                {
                    return new EndpointResponse<int>
                    {
                        Success = false,
                        Message = "No se puede eliminar la categoría porque tiene objetos asociados.",
                        Data = 0
                    };
                }

                // Eliminación lógica
                categoria.EsBorrado = true;
                _context.Update(categoria);
                await _context.SaveChangesAsync();

                return new EndpointResponse<int>
                {
                    Success = true,
                    Message = "La categoría fue eliminada correctamente.",
                    Data = id
                };
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en {nameof(Delete)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });

                return new EndpointResponse<int>
                {
                    Success = false,
                    Message = $"Error al eliminar la categoría: {ex.Message}",
                    Data = 0
                };
            }
        }




    }
}



    

