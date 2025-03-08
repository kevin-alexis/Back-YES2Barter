using Domain.Entities;
using Repository.Context;
using Repository.Seeders.SeedersServices.SeedersContracts;

namespace Repository.Seeders.SeedersServices.SeedersImplementation
{
    public class CategoriaSeeder : ISeeder
    {
        private readonly DataBaseContext _context;

        public CategoriaSeeder(DataBaseContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (!_context.Categorias.Any())
            {
                var categorias = new List<Categoria>
                {
                    new Categoria { Nombre = "Electrónica" },
                    new Categoria { Nombre = "Ropa y Moda" },
                    new Categoria { Nombre = "Hogar y Cocina" },
                    new Categoria { Nombre = "Deportes y Aire Libre" },
                    new Categoria { Nombre = "Salud y Belleza" },
                    new Categoria { Nombre = "Juguetes y Juegos" },
                    new Categoria { Nombre = "Automotriz" },
                    new Categoria { Nombre = "Libros y Papelería" }
                };

                _context.Categorias.AddRange(categorias);
                await _context.SaveChangesAsync();
            }
        }
    }
}
