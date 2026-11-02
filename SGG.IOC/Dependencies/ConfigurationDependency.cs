using Microsoft.Extensions.DependencyInjection;
using SGG.Application.Interfaces;
using SGG.Application.Services.Configuration;
using SGG.Persistence.Interfaces;
using SGG.Persistence.Repositories.Configuration;

namespace SGG.IOC.Dependencies
{
    public static class ConfigurationDependency
    {
        /// <summary>Módulo Configuration: Plan, Entrenador y Clase.</summary>
        public static IServiceCollection AddConfigurationDependencies(this IServiceCollection services)
        {
            // Repositorios
            services.AddScoped<IPlanRepository, PlanRepository>();
            services.AddScoped<IEntrenadorRepository, EntrenadorRepository>();
            services.AddScoped<IClaseRepository, ClaseRepository>();

            // Servicios de aplicación
            services.AddScoped<IPlanService, PlanService>();
            services.AddScoped<IEntrenadorService, EntrenadorService>();
            services.AddScoped<IClaseService, ClaseService>();

            return services;
        }
    }
}
