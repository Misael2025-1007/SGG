namespace SGG.Model.Dtos.Membership
{
    public class PagoDto
    {
        public int Id { get; set; }
        public int MembresiaId { get; set; }
        public decimal Monto { get; set; }
        public string? MetodoPago { get; set; }
        public DateTime FechaPago { get; set; }
        public bool Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
