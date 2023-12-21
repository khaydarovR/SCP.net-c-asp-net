namespace SCP.Domain.Entity
{
    public class ApiKeyWhiteIP : BaseEntity
    {
        public Guid ApiKeyId { get; set; }
        public ApiKey ApiKey { get; set; }
        public string AllowFrom { get; set; }
    }
}
