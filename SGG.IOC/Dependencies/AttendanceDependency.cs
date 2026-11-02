using Microsoft.Extensions.DependencyInjection;
using SGG.Application.Interfaces;
using SGG.Application.Services.Attendance;
using SGG.Persistence.Interfaces;
using SGG.Persistence.Repositories.Attendance;

namespace SGG.IOC.Dependencies
{
    public static class AttendanceDependency
    {
        /// <summary>Módulo Attendance: Asistencia y ReservaClase.</summary>
        public static IServiceCollection AddAttendanceDependencies(this IServiceCollection services)
        {
            // Repositorios
            services.AddScoped<IAsistenciaRepository, AsistenciaRepository>();
            services.AddScoped<IReservaClaseRepository, ReservaClaseRepository>();

            // Servicios de aplicación
            services.AddScoped<IAsistenciaService, AsistenciaService>();
            services.AddScoped<IReservaClaseService, ReservaClaseService>();

            return services;
        }
    }
}
