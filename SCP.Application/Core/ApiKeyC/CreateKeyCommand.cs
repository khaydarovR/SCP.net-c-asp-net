namespace SCP.Application.Core.ApiKeyC
{
    public class CreateKeyCommand
    {
        public Guid UserId { get; set; }
        public string Name { get; init; }
        public string SafeId { get; init; }
        public int DayLife { get; init; }
    }
}
