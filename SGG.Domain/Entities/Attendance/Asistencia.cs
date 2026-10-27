using SGG.Domain.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGG.Domain.Entities.Attendance
{
    [Table("Asistencia", Schema = "Attendance")]
    public class Asistencia : BaseEntity<int>
    {
        [Key]
        [Column("AsistenciaId")]
        public override int Id { get; set; }

        public int SocioId { get; set; }

        public int? ClaseId { get; set; }

        public DateTime FechaEntrada { get; set; }
    }
}
