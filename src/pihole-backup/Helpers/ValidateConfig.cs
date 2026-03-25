using Serilog;
using pihole_backup.Config;
using pihole_backup.Providers;

namespace pihole_backup.Helpers
{
    public static class ConfigValidator
    {
        public static Provider? Validate()
        {
            if (string.IsNullOrEmpty(AppConfig.Url) || string.IsNullOrEmpty(AppConfig.Password))
            {
                Log.Error("PIHOLE_URL and PIHOLE_PASSWORD environment variables are required.");
                return null;
            }

            if (!AppConfig.Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                Log.Warning("PIHOLE_URL does not use HTTPS. Credentials will be sent in cleartext.");
            }

            if (!Enum.TryParse(AppConfig.Provider, ignoreCase: true, out Provider provider))
            {
                Log.Error("Invalid PROVIDER: {Value}. Valid options: {Options}.",
                    AppConfig.Provider, string.Join(", ", Enum.GetNames<Provider>()));
                return null;
            }

            if (provider == Provider.Azure)
            {
                if (string.IsNullOrEmpty(AppConfig.AzureTenantId) || string.IsNullOrEmpty(AppConfig.AzureClientId)
                    || string.IsNullOrEmpty(AppConfig.AzureClientSecret) || string.IsNullOrEmpty(AppConfig.AzureStorageAccount)
                    || string.IsNullOrEmpty(AppConfig.AzureContainer))
                {
                    Log.Error("AZURE_TENANT_ID, AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, AZURE_STORAGE_ACCOUNT, and AZURE_CONTAINER environment variables are required.");
                    return null;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(AppConfig.S3AccessKey) || string.IsNullOrEmpty(AppConfig.S3SecretKey)
                    || string.IsNullOrEmpty(AppConfig.S3Bucket))
                {
                    Log.Error("S3_ACCESS_KEY, S3_SECRET_KEY, and S3_BUCKET environment variables are required.");
                    return null;
                }
            }

            return provider;
        }
    }
}
