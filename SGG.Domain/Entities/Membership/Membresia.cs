using SGG.Domain.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGG.Domain.Entities.Membership
{
    [Table("Membresia", Schema = "Membership")]
    public class Membresia : BaseEntity<int>
    {
        [Key]
        [Column("MembresiaId")]
        public override int Id { get; set; }

        public int SocioId { get; set; }

        public int PlanId { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaVencimiento { get; set; }
    }
}
