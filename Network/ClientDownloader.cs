using System.Diagnostics;
using System.Text;

namespace Network
{
    /// <summary>
    /// Provides methods to download content from the web using HTTP, additionally, it displays download progress
    /// </summary>
    public class ClientDownloader
    {
        private static HttpClient _client = new() { Timeout = Timeout.InfiniteTimeSpan };
        public async Task<byte[]> DownloadAsync(string url, string? fileName = null)
        {
            return await DownloadAndReportAsync(new Uri(url), fileName);
        }

        public async Task<byte[]> DownloadAsync(Uri uri, string? fileName = null)
        {
            return await DownloadAndReportAsync(uri, fileName);
        }

        public async Task<string> DownloadStringAsync(string url, string? fileName = null)
        {
            return Encoding.UTF8.GetString(await DownloadAndReportAsync(new Uri(url), fileName));
        }

        public async Task<string> DownloadStringAsync(Uri uri, string? fileName = null)
        {
            return Encoding.UTF8.GetString(await DownloadAndReportAsync(uri, fileName));
        }

        private async Task<byte[]> DownloadAndReportAsync(Uri uri, string? fileName = null)
        {
            const int BUF_SIZE = 512;
            byte[] bytes;

            using HttpResponseMessage response = await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

            byte[] buffer = new byte[BUF_SIZE];
            long downloadedBytes = 0;
            string progId;

            long totalBytes = response.Content.Headers.ContentLength ?? -1L;

            try
            {
                progId = uri.AbsoluteUri;
                ProgressReporterModel.RegisterProgress(uri.AbsoluteUri, totalBytes);
            } 
            catch (ArgumentException)
            {
                progId = uri.GetHashCode().ToString();
                ProgressReporterModel.RegisterProgress(uri.AbsoluteUri, totalBytes, progId);
            }

            using (var memStream = new MemoryStream())
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                long bytesRead = 0;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, BUF_SIZE)) > 0)
                {
                    downloadedBytes += bytesRead;
                    await memStream.WriteAsync(buffer);
                    ProgressReporterModel.ReportProgress(progId, downloadedBytes, false);
                }
                bytes = memStream.ToArray();

                if (totalBytes > downloadedBytes)
                {
                    throw new Exception($"Data missing or corrupted. Got {downloadedBytes} out of {bytes.Length} at URL {uri.AbsoluteUri}");
                }
                ProgressReporterModel.ReportProgress(progId, downloadedBytes, true);
            }
            return bytes;

        }
    }
}
