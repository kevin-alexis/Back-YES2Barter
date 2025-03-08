using Domain.Entities;
using Repository.Context;
using Repository.Seeders.SeedersServices.SeedersContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Enumerations.Enums;

namespace Repository.Seeders.SeedersServices.SeedersImplementation
{
    public class PropuestaIntercambioSeeder : ISeeder
    {
        private readonly DataBaseContext _context;

        public PropuestaIntercambioSeeder(DataBaseContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (!_context.PropuestasIntercambios.Any())
            {
                var propuestasIntercambios = new List<PropuestaIntercambio>
                {
                    // HAY QUE ARREGLARLOS
                    //new PropuestaIntercambio
                    //{
                    //    IdUsuarioOfertante = "1",
                    //    IdUsuarioReceptor = "user2",
                    //    IdObjetoOfertado = 1,
                    //    IdObjetoSolicitado = 2,
                    //    FechaPropuesta = DateOnly.FromDateTime(DateTime.UtcNow)
                    //},
                    //new PropuestaIntercambio
                    //{
                    //    IdUsuarioOfertante = "user3",
                    //    IdUsuarioReceptor = "user4",
                    //    IdObjetoOfertado = 2,
                    //    IdObjetoSolicitado = 3,
                    //    FechaPropuesta = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
                    //},
                    //new PropuestaIntercambio
                    //{
                    //    IdUsuarioOfertante = "user5",
                    //    IdUsuarioReceptor = "user6",
                    //    IdObjetoOfertado = 3,
                    //    IdObjetoSolicitado = 1,
                    //    FechaPropuesta = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2))
                    //}
                };

                _context.PropuestasIntercambios.AddRange(propuestasIntercambios);
                await _context.SaveChangesAsync();
            }
        }
    }
}
