using pihole_backup.Providers;

namespace pihole_backup.Models
{
    public class S3Options
    {
        public Provider Provider { get; set; } = Provider.AWS;
        public required string Region { get; set; }
        public required string Endpoint { get; set; }
        public required string AccessKey { get; set; }
        public required string SecretKey { get; set; }
        public required string Bucket { get; set; }
    }
}
