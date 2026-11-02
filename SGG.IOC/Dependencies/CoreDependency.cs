using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGG.Application.Configuration;
using SGG.Persistence.Context;

namespace SGG.IOC.Dependencies
{
    public static class CoreDependency
    {
        /// <summary>Registra el DbContext y las reglas de negocio configurables (sección "GymRules").</summary>
        public static IServiceCollection AddCoreDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SGGContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SGGContext")));

            services.Configure<GymRulesOptions>(configuration.GetSection(GymRulesOptions.SectionName));

            return services;
        }
    }
}
