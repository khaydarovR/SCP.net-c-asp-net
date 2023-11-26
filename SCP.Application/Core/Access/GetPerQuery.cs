namespace SCP.Application.Core.Access
{
    public class GetPerQuery
    {
        public Guid UserId { get; set; }
        public Guid SafeId { get; set; }
        public Guid? AuthorId { get; set; }
    }
}
