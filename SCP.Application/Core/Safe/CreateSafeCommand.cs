namespace SCP.Application.Core.Safe
{
    public class CreateSafeCommand
    {
        public Guid  UserId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string ClearKey { get; set; }
    }
}
