namespace SCP.Api.DTO
{
    public class CreateApiKeyDTO
    {
        public string Name { get; init; }
        public string SafeId { get; init; }
        public int DayLife { get; init; }
    }
}
