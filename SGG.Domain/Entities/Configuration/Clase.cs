using SGG.Domain.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGG.Domain.Entities.Configuration
{
    [Table("Clase", Schema = "Configuration")]
    public class Clase : BaseEntity<int>
    {
        [Key]
        [Column("ClaseId")]
        public override int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; }

        public int EntrenadorId { get; set; }

        public int Capacidad { get; set; }

        [Required, MaxLength(20)]
        public string DiaSemana { get; set; }

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFin { get; set; }
    }
}
