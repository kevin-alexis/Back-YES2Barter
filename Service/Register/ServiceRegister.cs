using Microsoft.Extensions.DependencyInjection;
using Service.Services.Contracts;
using Service.Services.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Register
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddProjectServices(this IServiceCollection services)
        {
            // Registrar los servicios
            services.AddScoped<IPersonaService, PersonaService>();

            return services;
        }
    }
}
