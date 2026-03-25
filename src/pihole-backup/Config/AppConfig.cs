namespace pihole_backup.Config
{
    public static class AppConfig
    {
        public static readonly string? LogLevel = Environment.GetEnvironmentVariable("LOG_LEVEL");
        public static readonly string Url = Environment.GetEnvironmentVariable("PIHOLE_URL") ?? string.Empty;
        public static readonly string Password = Environment.GetEnvironmentVariable("PIHOLE_PASSWORD") ?? string.Empty;
        public static readonly string Cron = Environment.GetEnvironmentVariable("BACKUP_CRON") ?? string.Empty;
        public static readonly string Provider = Environment.GetEnvironmentVariable("PROVIDER") ?? "AWS";
        public static readonly string S3Region = Environment.GetEnvironmentVariable("S3_REGION") ?? "eu-central-1";
        public static readonly string S3Endpoint = Environment.GetEnvironmentVariable("S3_ENDPOINT")
            is { Length: > 0 } endpoint
            ? endpoint
            : $"https://s3.{S3Region}.amazonaws.com";
        public static readonly string S3Bucket = Environment.GetEnvironmentVariable("S3_BUCKET") ?? string.Empty;
        public static readonly string S3AccessKey = Environment.GetEnvironmentVariable("S3_ACCESS_KEY") ?? string.Empty;
        public static readonly string S3SecretKey = Environment.GetEnvironmentVariable("S3_SECRET_KEY") ?? string.Empty;
        public static readonly string AzureTenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID") ?? string.Empty;
        public static readonly string AzureClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") ?? string.Empty;
        public static readonly string AzureClientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET") ?? string.Empty;
        public static readonly string AzureStorageAccount = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT") ?? string.Empty;
        public static readonly string AzureContainer = Environment.GetEnvironmentVariable("AZURE_CONTAINER") ?? string.Empty;
    }
}
