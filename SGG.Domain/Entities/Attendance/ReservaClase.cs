using SGG.Domain.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGG.Domain.Entities.Attendance
{
    [Table("ReservaClase", Schema = "Attendance")]
    public class ReservaClase : BaseEntity<int>
    {
        [Key]
        [Column("ReservaClaseId")]
        public override int Id { get; set; }

        public int SocioId { get; set; }

        public int ClaseId { get; set; }

        public DateTime FechaReserva { get; set; }

        public DateTime FechaClase { get; set; }
    }
}
