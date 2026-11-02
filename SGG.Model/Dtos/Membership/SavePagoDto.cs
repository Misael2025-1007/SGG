using SGG.Model.Dtos.Base;

namespace SGG.Model.Dtos.Membership
{
    public class SavePagoDto : DtoBase
    {
        public int MembresiaId { get; set; }
        public decimal Monto { get; set; }
        public DateTime? FechaPago { get; set; }
        public string? MetodoPago { get; set; }
    }
}
