namespace NCS.DSS.AdviserDetail.Models
{
    public class AdviserDetailConfigurationSettings
    {
        public required string CosmosDbEndpoint { get; set; }
        public required string AdviserDetailConnectionString { get; set; }
        public required string QueueName { get; set; }
        public required string ServiceBusConnectionString { get; set; }
        public required string DatabaseId { get; set; }
        public required string CollectionId { get; set; }
        public required string CustomerDatabaseId { get; set; }
        public required string CustomerCollectionId { get; set; }
    }
}
