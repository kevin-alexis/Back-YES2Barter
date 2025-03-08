using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace Service.Services
{
    public class MensajeService : IMensajeService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public MensajeService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MensajeDTO>> ObtenerMensajesAsync(int emisorId, int receptorId)
        {
            var mensajes = await _context.Mensajes
                .Where(m => (m.IdEmisor == emisorId && m.IdReceptor == receptorId) ||
                            (m.IdEmisor == receptorId && m.IdReceptor == emisorId))
                .OrderBy(m => m.FechaEnvio)
                .ToListAsync();

            return _mapper.Map<IEnumerable<MensajeDTO>>(mensajes);
        }

        public async Task EnviarMensajeAsync(MensajeDTO mensajeDTO)
        {
            var mensaje = _mapper.Map<Mensaje>(mensajeDTO);
            _context.Mensajes.Add(mensaje);
            await _context.SaveChangesAsync();
        }
    }
}
