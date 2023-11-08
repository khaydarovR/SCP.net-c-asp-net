namespace SCP.Api.DTO
{
    public class CreateRecordDTO
    {
        public string Title { get; set; }
        public string Login { get; set; }
        public string Pw { get; set; }
        public string Secret { get; set; }
        public string SafeId { get; set; }
    }
}
