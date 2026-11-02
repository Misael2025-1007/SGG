namespace SGG.Model.Dtos.Attendance
{
    public class ReservaClaseDto
    {
        public int Id { get; set; }
        public int SocioId { get; set; }
        public int ClaseId { get; set; }
        public DateTime FechaClase { get; set; }
        public DateTime FechaReserva { get; set; }
        public bool Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
