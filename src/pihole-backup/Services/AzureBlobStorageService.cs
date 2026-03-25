using Azure.Identity;
using Azure.Storage.Blobs;
using Serilog;
using pihole_backup.Models;

namespace pihole_backup.Services
{
    public class AzureBlobStorageService : IStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly AzureBlobOptions _options;

        public AzureBlobStorageService(AzureBlobOptions options)
        {
            _options = options;

            ClientSecretCredential credential = new(
                options.TenantId,
                options.ClientId,
                options.ClientSecret
            );

            BlobServiceClient serviceClient = new(
                new Uri($"https://{options.StorageAccount}.blob.core.windows.net"),
                credential
            );

            _containerClient = serviceClient.GetBlobContainerClient(options.Container);

            Log.Information("Azure Blob client initialized for storage account {Account}, container {Container}.",
                options.StorageAccount, options.Container);
        }

        public async Task UploadAsync(string blobName, string filePath)
        {
            Log.Information("Uploading {Blob} to container {Container}...", blobName, _options.Container);

            try
            {
                BlobClient blobClient = _containerClient.GetBlobClient(blobName);

                await blobClient.UploadAsync(filePath, overwrite: true);

                Log.Information("Successfully uploaded {Blob} to container {Container}.", blobName, _options.Container);
            }
            catch (Azure.RequestFailedException ex)
            {
                Log.Error(ex, "Azure Blob error uploading {Blob} to container {Container}. Status: {Status}.",
                    blobName, _options.Container, ex.Status);
                throw;
            }
        }
    }
}
