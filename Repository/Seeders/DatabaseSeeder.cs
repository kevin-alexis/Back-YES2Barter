using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Context;
using Domain.Entities;

namespace Repository.Seeders
{
    public static class DatabaseSeeder
    {
        public static void Seed(AppDbContext context)
        {
            if (!context.Usuarios.Any())
            {
                context.Usuarios.AddRange(
                    new Usuario { IdUsuario = 1, Nombre = "Alice" },
                    new Usuario { IdUsuario = 2, Nombre = "Bob" }
                );
                context.SaveChanges();
            }
        }
    }
}
