using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGG.IOC.Dependencies;

namespace SGG.IOC
{
    public static class DependencyInjection
    {
        /// <summary>Punto único de registro de dependencias del SGG para las capas de presentación (Api / Web).</summary>
        public static IServiceCollection AddSGGDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCoreDependencies(configuration);
            services.AddConfigurationDependencies();
            services.AddMembershipDependencies();
            services.AddAttendanceDependencies();

            return services;
        }
    }
}
