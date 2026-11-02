namespace SGG.Model.Dtos.Attendance
{
    public class AsistenciaDto
    {
        public int Id { get; set; }
        public int SocioId { get; set; }
        public int? ClaseId { get; set; }
        public DateTime FechaEntrada { get; set; }
        public bool Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
