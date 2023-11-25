namespace SCP.Application.Core.Record
{
    public class ReadRecordCommand
    {
        public string PubKeyFromClient {  get; set; }
        public Guid RecordId {  get; set; }
        public Guid AuthorId {  get; set; }
    }
}
