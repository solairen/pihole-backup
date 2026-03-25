using pihole_backup.Services;
using Serilog;
using Cronos;
using pihole_backup.Providers;
using pihole_backup.Models;
using pihole_backup.Config;
using pihole_backup.Helpers;

namespace pihole_backup
{
    internal class Program
    {
        private static Provider _provider;
        private static readonly string version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";

        private static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(Enum.Parse<Serilog.Events.LogEventLevel>(AppConfig.LogLevel ?? "Information"))
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {Message}{NewLine}{Exception}")
                .CreateLogger();

            try
            {
                Provider? result = ConfigValidator.Validate();
                if (result is not Provider validProvider) return;
                _provider = validProvider;

                if (string.IsNullOrEmpty(AppConfig.Cron))
                    await RunAsync();
                else
                    await RunOnCronAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Unhandled exception.");
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }

        private static async Task RunOnCronAsync()
        {
            CronExpression expression = CronExpression.Parse(AppConfig.Cron);

            using CancellationTokenSource cts = new();

            Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

            Log.Information("############## PIHOLE BACKUP ##############");
            Log.Information("VERSION:                  {Version}", version);
            Log.Information("SCHEDULER:                {Cron}", AppConfig.Cron);
            Log.Information("PIHOLE URL:               {Url}", AppConfig.Url);
            Log.Information("PROVIDER:                 {Provider}", _provider);

            if (_provider == Provider.Azure)
            {
                Log.Information("AZURE STORAGE ACCOUNT:    {AzureStorageAccount}", AppConfig.AzureStorageAccount);
                Log.Information("AZURE CONTAINER:          {AzureContainer}", AppConfig.AzureContainer);
            }
            else
            {
                Log.Information("S3 REGION:                {S3Region}", AppConfig.S3Region);
                Log.Information("S3 ENDPOINT:              {S3Endpoint}", AppConfig.S3Endpoint);
                Log.Information("S3 BUCKET:                {S3Bucket}", AppConfig.S3Bucket);
            }

            Log.Information("Press Ctrl+C to exit.");

            DateTimeOffset from = DateTimeOffset.Now;

            while (!cts.Token.IsCancellationRequested)
            {
                var next = expression.GetNextOccurrence(from, TimeZoneInfo.Local);
                if (next == null) break;

                TimeSpan delay = next.Value - DateTimeOffset.Now;
                Log.Information("-------------------------------------------");
                Log.Information("Next backup at {NextRun}.", next.Value.ToString("yyyy-MM-dd HH:mm:ss"));

                try
                {
                    await Task.Delay(delay, cts.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                try
                {
                    await RunAsync(next.Value);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Backup run failed. Will retry on next schedule.");
                }

                from = next.Value;
            }

            Log.Information("Scheduler stopped.");
        }

        private static async Task RunAsync(DateTimeOffset? scheduled = null)
        {
            string date = (scheduled ?? DateTimeOffset.Now).ToString("yyyy-MM-dd_HH-mm");
            string filePath = $"teleporter-{date}.tar.gz";
            string key = $"pihole/teleporter-{date}.tar.gz";

            PiHoleService pihole = new();
            bool success = await pihole.RunBackupAsync(AppConfig.Url, AppConfig.Password, filePath);
            if (!success) return;

            IStorageService storage = _provider == Provider.Azure
                ? new AzureBlobStorageService(new AzureBlobOptions
                {
                    TenantId = AppConfig.AzureTenantId,
                    ClientId = AppConfig.AzureClientId,
                    ClientSecret = AppConfig.AzureClientSecret,
                    StorageAccount = AppConfig.AzureStorageAccount,
                    Container = AppConfig.AzureContainer,
                })
                : new AWSS3Service(new S3Options
                {
                    Provider = _provider,
                    Region = AppConfig.S3Region,
                    Endpoint = AppConfig.S3Endpoint,
                    AccessKey = AppConfig.S3AccessKey,
                    SecretKey = AppConfig.S3SecretKey,
                    Bucket = AppConfig.S3Bucket,
                });

            await storage.UploadAsync(key, filePath);

            if (storage is IDisposable disposable)
                disposable.Dispose();

            File.Delete(filePath);
            Log.Information("Deleted local file {FilePath} after upload.", filePath);
        }
    }
}
