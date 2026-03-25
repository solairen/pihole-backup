using RestSharp;
using Serilog;
using System.Text.Json;

namespace pihole_backup.Services
{
    public class PiHoleService : IDisposable
    {
        private readonly RestClient _client = new();

        public async Task<bool> RunBackupAsync(string url, string password, string filePath)
        {
            string sid = await LoginAsync(url, password);
            if (string.IsNullOrEmpty(sid))
            {
                Log.Error("Login failed. No SID returned.");
                return false;
            }

            bool backup = await DownloadBackupAsync(url, sid, filePath);
            if (backup)
                Log.Information("Backup created successfully at {FilePath}.", filePath);
            else
                Log.Error("Backup failed.");

            return backup;
        }

        private async Task<string> LoginAsync(string url, string password)
        {
            try
            {
                RestRequest request = new($"{url}/api/auth", Method.Post);
                request.AddJsonBody(new { password });
                RestResponse response = await _client.ExecuteAsync(request);

                Log.Debug("Login request sent to {Url}.", $"{url}/api/auth");

                if (response.Content == null)
                    return string.Empty;

                using JsonDocument doc = JsonDocument.Parse(response.Content);

                if (!response.IsSuccessful)
                {
                    if (doc.RootElement.TryGetProperty("session", out var sessionElem)
                        && sessionElem.TryGetProperty("message", out var messageElem))
                    {
                        Log.Error("Failed to login to PiHole. Status: {StatusCode}, Message: {Message}.",
                            response.StatusCode, messageElem.GetString());
                    }
                    return string.Empty;
                }

                if (doc.RootElement.TryGetProperty("session", out var sess)
                    && sess.TryGetProperty("sid", out var sidElem))
                {
                    return sidElem.GetString() ?? string.Empty;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to login to PiHole at {Url}.", url);
                return string.Empty;
            }
        }

        private async Task<bool> DownloadBackupAsync(string url, string sid, string filePath)
        {
            try
            {
                RestRequest request = new($"{url}/api/teleporter", Method.Get);

                request.AddHeader("sid", sid);
                request.AddHeader("Accept", "application/zip");

                RestResponse response = await _client.ExecuteAsync(request);

                Log.Debug("Backup request sent to {Url}.", $"{url}/api/teleporter");

                if (!response.IsSuccessful)
                {
                    Log.Error("Backup request failed. Status: {StatusCode}.", response.StatusCode);
                    return false;
                }

                if (response.RawBytes != null)
                {
                    await File.WriteAllBytesAsync(filePath, response.RawBytes);
                    return true;
                }

                Log.Error("No content received from PiHole backup endpoint.");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to download backup from {Url}.", url);
                return false;
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
