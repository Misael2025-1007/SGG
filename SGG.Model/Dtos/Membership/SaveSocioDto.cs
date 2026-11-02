using SGG.Model.Dtos.Base;

namespace SGG.Model.Dtos.Membership
{
    public class SaveSocioDto : DtoBase
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string? Cedula { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public DateTime? FechaNacimiento { get; set; }
    }
}
