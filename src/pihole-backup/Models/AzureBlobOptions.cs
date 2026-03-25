namespace pihole_backup.Models
{
    public class AzureBlobOptions
    {
        public required string TenantId { get; set; }
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        public required string StorageAccount { get; set; }
        public required string Container { get; set; }
    }
}
