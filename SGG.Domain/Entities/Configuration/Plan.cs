using SGG.Domain.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGG.Domain.Entities.Configuration
{
    [Table("Plan", Schema = "Configuration")]
    public class Plan : BaseEntity<int>
    {
        [Key]
        [Column("PlanId")]
        public override int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; }

        [MaxLength(250)]
        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        public int DuracionDias { get; set; }
    }
}
