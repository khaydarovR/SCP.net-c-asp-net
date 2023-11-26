using System.Diagnostics.CodeAnalysis;

namespace SCP.Api.DTO
{
    public class PatchPerDTO
    {
        public string UserId { get; set; }
        public string SafeId { get; set; }
        public List<string> PermissionSlags { get; set; }
        [MaybeNull]
        public int? DayLife { get; set; }
    }
}
