using Amazon.S3;
using Amazon.S3.Model;
using Serilog;
using pihole_backup.Providers;
using pihole_backup.Models;

namespace pihole_backup.Services
{
    public sealed class AWSS3Service : IStorageService, IDisposable
    {
        private readonly AmazonS3Client _s3Client;
        private readonly S3Options _options;

        public AWSS3Service(S3Options options)
        {
            _options = options;
            _s3Client = CreateClient(options);
            Log.Information("S3 client initialized for {Provider} at {Endpoint}.",
                options.Provider, options.Endpoint);
        }

        public async Task UploadAsync(string key, string filePath)
        {
            Log.Information("Uploading {Key} to bucket {Bucket}...", key, _options.Bucket);

            try
            {
                bool isAWS = _options.Provider == Provider.AWS;

                PutObjectRequest request = new()
                {
                    BucketName = _options.Bucket,
                    Key = key,
                    FilePath = filePath,
                    DisablePayloadSigning = false,
                    UseChunkEncoding = isAWS,
                };

                PutObjectResponse response = await _s3Client.PutObjectAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    Log.Information("Successfully uploaded {Key} to bucket {Bucket}.", key, _options.Bucket);
                else
                    Log.Warning("Upload of {Key} returned status {StatusCode}.", key, response.HttpStatusCode);
            }
            catch (AmazonS3Exception ex)
            {
                Log.Error(ex, "S3 error uploading {Key} to bucket {Bucket}.", key, _options.Bucket);
                throw;
            }
        }

        private static AmazonS3Client CreateClient(S3Options options)
        {
            var config = new AmazonS3Config();
            if (options.Provider == Provider.AWS)
            {
                config.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(options.Region);
            }
            else
            {
                config.ServiceURL = options.Endpoint;
                config.AuthenticationRegion = options.Region;
                config.ForcePathStyle = true;
            }
            return new AmazonS3Client(options.AccessKey, options.SecretKey, config);
        }

        public void Dispose()
        {
            _s3Client.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
