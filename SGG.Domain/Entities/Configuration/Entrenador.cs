using SGG.Domain.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGG.Domain.Entities.Configuration
{
    [Table("Entrenador", Schema = "Configuration")]
    public class Entrenador : BaseEntity<int>
    {
        [Key]
        [Column("EntrenadorId")]
        public override int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; }

        [Required, MaxLength(100)]
        public string Apellido { get; set; }

        [MaxLength(100)]
        public string? Especialidad { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }
    }
}
