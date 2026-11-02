namespace SGG.Model.Dtos.Configuration
{
    public class ClaseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int EntrenadorId { get; set; }
        public int Capacidad { get; set; }
        public string DiaSemana { get; set; } = string.Empty;
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public bool Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
