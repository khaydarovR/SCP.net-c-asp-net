namespace SCP.Application.Common.Response
{
    public class ReadRecordResponse
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string ESecret { get; set; }
        public string ELogin { get; set; }
        public string EPw { get; set; }

        public string ForResource { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;
    }
}
