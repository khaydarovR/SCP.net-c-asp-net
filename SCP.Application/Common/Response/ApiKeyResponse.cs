namespace SCP.Application.Common.Response
{
    public class ApiKeyResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public DateTime DeadDate { get; set; }
        public Guid SafeId { get; set; }
        public string SafeName { get; set; }
    }
}
