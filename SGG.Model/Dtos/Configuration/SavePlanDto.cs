using SGG.Model.Dtos.Base;

namespace SGG.Model.Dtos.Configuration
{
    public class SavePlanDto : DtoBase
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int DuracionDias { get; set; }
    }
}
