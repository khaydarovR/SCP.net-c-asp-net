namespace SCP.Application.Common.Response
{
    public class GetLinkedSafeResponse
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
    }

}
