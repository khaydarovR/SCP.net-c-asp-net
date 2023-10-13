namespace SCP.Domain.Entity
{
    public class Bot : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string EApiKey { get; set; }

        public Guid OwnerId {  get; set; }

        public ICollection<BotRight> Rights { get; set; }

    }
}
