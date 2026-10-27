using Microsoft.EntityFrameworkCore;
using SGG.Domain.Entities.Configuration;
using SGG.Domain.Entities.Membership;
using SGG.Domain.Entities.Attendance;

namespace SGG.Persistence.Context
{
    public class SGGContext : DbContext
    {
        public SGGContext(DbContextOptions<SGGContext> options) : base(options)
        {
        }

        public DbSet<Plan> Plans { get; set; }
        public DbSet<Entrenador> Entrenadors { get; set; }
        public DbSet<Clase> Clases { get; set; }
        public DbSet<Socio> Socios { get; set; }
        public DbSet<Membresia> Membresias { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Asistencia> Asistencias { get; set; }
        public DbSet<ReservaClase> ReservaClases { get; set; }
    }
}
