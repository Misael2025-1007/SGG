using SGG.Model.Dtos.Base;

namespace SGG.Model.Dtos.Membership
{
    public class SaveMembresiaDto : DtoBase
    {
        public int SocioId { get; set; }
        public int PlanId { get; set; }
        public DateTime? FechaInicio { get; set; }
    }
}
