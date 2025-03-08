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
    public class ObjetoSeeder : ISeeder
    {
        private readonly DataBaseContext _context;

        public ObjetoSeeder(DataBaseContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (!_context.Objetos.Any())
            {
                var objetos = new List<Objeto>
                {
                   new Objeto
                    {
                        Nombre = "Xbox",
                        Descripcion = "Consola de videojuegos de última generación.",
                        FechaPublicacion = DateOnly.FromDateTime(DateTime.Now),
                        IdCategoria = 1,
                        RutaImagen = "ruta/imagen1.jpg",
                    },
                    new Objeto
                    {
                        Nombre = "Playera",
                        Descripcion = "Playera de algodón, cómoda y elegante.",
                        FechaPublicacion = DateOnly.FromDateTime(DateTime.Now),
                        IdCategoria = 2,
                        RutaImagen = "ruta/imagen2.jpg",
                    },
                    new Objeto
                    {
                        Nombre = "Cacerola",
                        Descripcion = "Cacerola antiadherente ideal para cocina gourmet.",
                        FechaPublicacion = DateOnly.FromDateTime(DateTime.Now),
                        IdCategoria = 3,
                        RutaImagen = "ruta/imagen3.jpg",
                    }
                };

                _context.Objetos.AddRange(objetos);
                await _context.SaveChangesAsync();
            }
        }
    }
}
