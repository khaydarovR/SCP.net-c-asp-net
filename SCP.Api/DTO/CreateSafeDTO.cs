namespace SCP.Api.DTO
{
    public class CreateSafeDTO
    {
        public string Title { get; init; }
        public string? Description { get; init; }
        public string? ClearKey { get; init; }
    }
}
