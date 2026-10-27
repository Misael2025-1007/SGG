using SGG.Domain.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGG.Domain.Entities.Membership
{
    [Table("Pago", Schema = "Membership")]
    public class Pago : BaseEntity<int>
    {
        [Key]
        [Column("PagoId")]
        public override int Id { get; set; }

        public int MembresiaId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Monto { get; set; }

        public DateTime FechaPago { get; set; }

        [MaxLength(50)]
        public string? MetodoPago { get; set; }
    }
}
