using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
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

        public BaseService(DataBaseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<IEnumerable<TDto>> GetAll()
        {
            try
            {
                var items = await _dbSet.Where(e => EF.Property<bool>(e, "EsBorrado") == false).ToListAsync();
                return _mapper.Map<IEnumerable<TDto>>(items);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener todos los elementos", ex);
            }
        }

        public virtual async Task<TDto> GetById(int id)
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
                throw new Exception($"Error al obtener el elemento con id {id}", ex);
            }
        }

        public virtual async Task Add(TDto itemDto)
        {
            try
            {
                var item = _mapper.Map<T>(itemDto);
                await _dbSet.AddAsync(item);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar el elemento", ex);
            }
        }

        public virtual async Task Update(TDto itemDto)
        {
            try
            {
                var item = _mapper.Map<T>(itemDto);
                _dbSet.Update(item);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar el elemento", ex);
            }
        }

        public virtual async Task Delete(int id)
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
                throw new Exception($"Error al eliminar el elemento con id {id}", ex);
            }
        }
    }
}
