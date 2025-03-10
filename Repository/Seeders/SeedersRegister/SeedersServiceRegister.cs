using Microsoft.Extensions.DependencyInjection;
using Repository.Seeders.SeedersServices.SeedersContracts;
using Repository.Seeders.SeedersServices.SeedersImplementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Seeders.SeedersRegister
{
    public static class SeedersServiceRegister
    {
        public static IServiceCollection AddSeeders(this IServiceCollection services)
        {
            // Registrar los seeders
            services.AddTransient<ISeeder, CategoriaSeeder>();
            services.AddTransient<ISeeder, PersonaSeeder>();
            services.AddTransient<ISeeder, ObjetoSeeder>();
            services.AddTransient<ISeeder, PropuestaIntercambioSeeder>();
            services.AddTransient<DataBaseSeeder>();

            return services;
        }
    }
}
