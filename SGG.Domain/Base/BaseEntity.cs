namespace SGG.Domain.Base
{
    public abstract class BaseEntity<TType> : AuditEntity
    {
        public abstract TType Id { get; set; }
    }
}
