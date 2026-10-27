namespace SGG.Domain.Base
{
    public abstract class AuditEntity
    {
        protected AuditEntity()
        {
            CreationDate = DateTime.Now;
            Status = true;
        }
        public DateTime CreationDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public int? UserMod { get; set; }
        public bool Status { get; set; }
    }
}
