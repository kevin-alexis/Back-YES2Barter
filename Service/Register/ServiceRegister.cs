using Microsoft.Extensions.DependencyInjection;
using Service.Services.Contracts;
using Service.Services.Implementation;


namespace Service.Register
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddProjectServices(this IServiceCollection services)
        {
            // Registrar los servicios
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IObjetoService, ObjetoService>();
            services.AddScoped<ICategoriaService, CategoriaService>();
            services.AddScoped<IPropuestaIntercambioService, PropuestaIntercambioService>();
            services.AddScoped<IPersonaService, PersonaService>();
            services.AddScoped<IEstadisticaService, EstadisticaService>();
            services.AddScoped<IMensajeService, MensajeService>();

            return services;
        }
    }
}
