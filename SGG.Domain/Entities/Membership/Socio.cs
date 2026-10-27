using SGG.Domain.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGG.Domain.Entities.Membership
{
    [Table("Socio", Schema = "Membership")]
    public class Socio : BaseEntity<int>
    {
        [Key]
        [Column("SocioId")]
        public override int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; }

        [Required, MaxLength(100)]
        public string Apellido { get; set; }

        [MaxLength(20)]
        public string? Cedula { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }

        public DateTime? FechaNacimiento { get; set; }
    }
}
