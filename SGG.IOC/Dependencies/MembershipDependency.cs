using Microsoft.Extensions.DependencyInjection;
using SGG.Application.Interfaces;
using SGG.Application.Services.Membership;
using SGG.Persistence.Interfaces;
using SGG.Persistence.Repositories.Membership;

namespace SGG.IOC.Dependencies
{
    public static class MembershipDependency
    {
        /// <summary>Módulo Membership: Socio, Membresia y Pago.</summary>
        public static IServiceCollection AddMembershipDependencies(this IServiceCollection services)
        {
            // Repositorios
            services.AddScoped<ISocioRepository, SocioRepository>();
            services.AddScoped<IMembresiaRepository, MembresiaRepository>();
            services.AddScoped<IPagoRepository, PagoRepository>();

            // Servicios de aplicación
            services.AddScoped<ISocioService, SocioService>();
            services.AddScoped<IMembresiaService, MembresiaService>();
            services.AddScoped<IPagoService, PagoService>();

            return services;
        }
    }
}
