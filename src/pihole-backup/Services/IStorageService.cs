namespace pihole_backup.Services
{
    public interface IStorageService
    {
        Task UploadAsync(string key, string filePath);
    }
}
