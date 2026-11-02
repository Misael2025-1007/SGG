using SGG.Model.Dtos.Base;

namespace SGG.Model.Dtos.Configuration
{
    public class SaveEntrenadorDto : DtoBase
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string? Especialidad { get; set; }
        public string? Telefono { get; set; }
    }
}
