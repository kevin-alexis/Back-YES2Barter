using Domain.Entities;
using Repository.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Logging
{
    public class Logger
    {
        private readonly DataBaseContext _context;

        public Logger(DataBaseContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string nivel, string mensaje, string excepcion = null)
        {
            var log = new Log
            {
                Fecha = DateTime.Now,
                Nivel = nivel,
                Mensaje = mensaje,
                Excepcion = excepcion
            };

            await _context.Logs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
