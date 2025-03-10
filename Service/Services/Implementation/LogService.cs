using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using Service.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class LogService : ILogService 
    {
        private readonly DataBaseContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        public LogService(DataBaseContext context, IMapper mapper, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LogDTO>> GetAll()
        {
            try
            {
                var items = await _context.Logs.ToListAsync();
                return _mapper.Map<IEnumerable<LogDTO>>(items);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener todos los elementos", ex);
            }
        }

        public async Task AddAsync(LogDTO logDTO)
        {
            try
            {
                logDTO.Fecha = DateTime.Now;
                if (!string.IsNullOrEmpty(logDTO.Excepcion))
                {
                    if (logDTO.Excepcion.Contains("NullReferenceException"))
                    {
                        logDTO.Nivel = "Error"; 
                    }
                    else if (logDTO.Excepcion.Contains("ArgumentException"))
                    {
                        logDTO.Nivel = "Warning";
                    }
                    else if (logDTO.Excepcion.Contains("InvalidOperationException"))
                    {
                        logDTO.Nivel = "Error";
                    }
                    else
                    {
                        logDTO.Nivel = "Info";
                    }
                }
                else
                {
                    logDTO.Nivel = "Info";
                }
                var item = _mapper.Map<Log>(logDTO);
                await _context.Logs.AddAsync(item);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar el elemento", ex);
            }
        }

        public async Task<LogDTO> GetById(int id)
        {
            try
            {
                var log = await _context.Logs.Where(e => e.Id == id).FirstOrDefaultAsync();
                if (log != null)
                {
                    return _mapper.Map<LogDTO>(log);
                }
                return default(LogDTO);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener el elemento con id {id}", ex);
            }
        }


    }
}
