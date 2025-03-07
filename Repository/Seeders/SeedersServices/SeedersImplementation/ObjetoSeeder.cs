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
                        Nombre = "One Piece.",
                        Descripcion = "La obra narra las aventuras de Monkey D. Luffy y su tripulación, los Piratas de Sombrero de Paja, recorriendo el mar para encontrar el legendario tesoro One Piece.",
                        FechaPublicacion = DateOnly.FromDateTime(DateTime.Now),
                        IdCategoria = 1,
                        RutaImagen = "ruta/imagen1.jpg",
                    },
                    new Objeto
                    {
                        Nombre = "Lainist: La Pesadilla de la Fabricación.",
                        Descripcion = "La joven Lain Iwakura inicia un camino que la llevará a adentrarse cada vez más en la red Wired.",
                        FechaPublicacion = DateOnly.FromDateTime(DateTime.Now),
                        IdCategoria = 2,
                        RutaImagen = "ruta/imagen2.jpg",
                    },
                    new Objeto
                    {
                        Nombre = "Darling In The Franxx.",
                        Descripcion = "Los supervivientes de una guerra encarnizada contra unos monstruos se esconden dentro de ciudades fortificadas móviles y entrenan a los jóvenes para pilotar grandes robots de batalla llamados Franxx en defensa de la humanidad.",
                        FechaPublicacion = DateOnly.FromDateTime(DateTime.Now),
                        IdCategoria = 3,
                        RutaImagen = "ruta/imagen3.jpg",
                    },
                };

                _context.Objetos.AddRange(objetos);
                await _context.SaveChangesAsync();
            }
        }
    }
}
