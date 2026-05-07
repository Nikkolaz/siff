namespace SSF.Interop.SIIFNacion.Domain.Common
{
    public abstract class BaseDomainEntity
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public Guid CUS { get; set; }
        public string? TransactionId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool IsDelete { get; set; }
    }
}
