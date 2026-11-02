namespace SGG.Model.Dtos.Configuration
{
    public class EntrenadorDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string? Especialidad { get; set; }
        public string? Telefono { get; set; }
        public bool Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
