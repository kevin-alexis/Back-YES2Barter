using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Context;
using Service.Logging;
using Service.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class BaseService<T, TDto> : IBaseService<T, TDto> where T : BaseEntity
    {
        protected readonly DataBaseContext _context;
        protected readonly IMapper _mapper;
        protected readonly DbSet<T> _dbSet;
        private readonly ILogService _logger;

        public BaseService(DataBaseContext context, IMapper mapper, ILogService logger)
        {
            _context = context;
            _mapper = mapper;
            _dbSet = _context.Set<T>();
            _logger = logger;
        }

        virtual public async Task<IEnumerable<TDto>> GetAll()
        {
            try
            {
                var items = await _dbSet.Where(e => EF.Property<bool>(e, "EsBorrado") == false).ToListAsync();
                return _mapper.Map<IEnumerable<TDto>>(items);
            }
            catch (Exception ex)
            {
                await _logger.AddAsync(new Domain.DTOs.LogDTO { Nivel = "Error", Mensaje = "Error al obtener todos los elementos", Excepcion = ex.ToString() });
                throw new Exception("Error al obtener todos los elementos", ex);
            }
        }

        virtual public async Task<TDto> GetById(int id)
        {
            try
            {
                var item = await _dbSet.Where(e => e.Id == id && !EF.Property<bool>(e, "EsBorrado")).FirstOrDefaultAsync();
                if (item != null)
                {
                    return _mapper.Map<TDto>(item);
                }
                return default(TDto);
            }
            catch (Exception ex)
            {
                await _logger.AddAsync(new Domain.DTOs.LogDTO { Nivel = "Error", Mensaje = $"Error al obtener el elemento con id {id}", Excepcion = ex.ToString() });
                throw new Exception($"Error al obtener el elemento con id {id}", ex);
            }
        }

        virtual public async Task Add(TDto itemDto)
        {
            try
            {
                var item = _mapper.Map<T>(itemDto);
                await _dbSet.AddAsync(item);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await _logger.AddAsync(new Domain.DTOs.LogDTO { Nivel = "Error", Mensaje = "Error al agregar el elemento", Excepcion = ex.ToString() });
                throw new Exception("Error al agregar el elemento", ex);
            }
        }

        virtual public async Task Update(TDto itemDto)
        {
            try
            {
                var item = _mapper.Map<T>(itemDto);

                var trackedEntity = _context.Set<T>().Local.FirstOrDefault(e => e.Id == item.Id);
                if (trackedEntity != null)
                {
                    _context.Entry(trackedEntity).State = EntityState.Detached;
                }

                _dbSet.Update(item);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await _logger.AddAsync(new Domain.DTOs.LogDTO { Nivel = "Error", Mensaje = "Error al actualizar el elemento", Excepcion = ex.ToString() });
                throw new Exception("Error al actualizar el elemento", ex);
            }
        }

        virtual public async Task Delete(int id)
        {
            try
            {
                var item = await _dbSet.FindAsync(id);
                if (item != null)
                {
                    var esBorradoProperty = item.GetType().GetProperty("EsBorrado");
                    if (esBorradoProperty != null)
                    {
                        esBorradoProperty.SetValue(item, true);
                        _dbSet.Update(item);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.AddAsync(new Domain.DTOs.LogDTO { Nivel = "Error", Mensaje = $"Error al eliminar el elemento con id {id}", Excepcion = ex.ToString() });
                throw new Exception($"Error al eliminar el elemento con id {id}", ex);
            }
        }
    }
}
