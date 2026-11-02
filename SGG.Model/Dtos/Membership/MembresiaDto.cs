namespace SGG.Model.Dtos.Membership
{
    public class MembresiaDto
    {
        public int Id { get; set; }
        public int SocioId { get; set; }
        public int PlanId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public bool Vigente { get; set; }
        public bool Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
