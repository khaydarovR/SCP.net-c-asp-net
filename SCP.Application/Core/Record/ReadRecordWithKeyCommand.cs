namespace SCP.Application.Core.Record
{
    public class ReadRecordWithKeyCommand
    {
        public string PubKeyFromClient { get; set; }
        public string RecordId { get; set; }
        public Guid AuthorId { get; set; }
    }
}
