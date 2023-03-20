using System.Text;

namespace Network
{
    /// <summary>
    /// Provides methods to download content from the web using HTTP, additionally, it displays download progress
    /// </summary>
    public class Downloader
    {
        public async Task<byte[]> DownloadAsync(string url, string? fileName = null)
        {
            var progress = new Progress<SingleDlProgressInfo>();
            progress.ProgressChanged += DownloadProgressChanged;
            return await DownloadAndReportAsync(new Uri(url), progress, fileName);
        }

        public async Task<byte[]> DownloadAsync(Uri uri, string? fileName = null)
        {
            var progress = new Progress<SingleDlProgressInfo>();
            progress.ProgressChanged += DownloadProgressChanged;
            return await DownloadAndReportAsync(uri, progress, fileName);
        }

        public async Task<string> DownloadStringAsync(string url, string? fileName = null)
        {
            var progress = new Progress<SingleDlProgressInfo>();
            progress.ProgressChanged += DownloadProgressChanged;
            return Encoding.UTF8.GetString(await DownloadAndReportAsync(new Uri(url), progress, fileName));
        }

        public async Task<string> DownloadStringAsync(Uri uri, string? fileName = null)
        {
            var progress = new Progress<SingleDlProgressInfo>();
            progress.ProgressChanged += DownloadProgressChanged;
            return Encoding.UTF8.GetString(await DownloadAndReportAsync(uri, progress, fileName));
        }

        private async Task<byte[]> DownloadAndReportAsync(Uri uri, IProgress<SingleDlProgressInfo> progress, string? fileName = null)
        {
            const int BUF_SIZE = 1024;
            byte[] bytes;
            using (var client = new HttpClient(new HttpClientHandler() { MaxConnectionsPerServer = 1 }))
            {
                HttpResponseMessage initResponse = await client.GetAsync(uri);

                byte[] buffer = new byte[BUF_SIZE];
                long downloadedBytes = 0;

                using (var memStream = new MemoryStream())
                using (var stream = await initResponse.Content.ReadAsStreamAsync())
                {
                    long bytesRead = 0;
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, BUF_SIZE)) > 0)
                    {
                        downloadedBytes += bytesRead;
                        progress?.Report(new SingleDlProgressInfo()
                        {
                            Name = fileName == null ? uri.AbsoluteUri : fileName,
                            PercentComplete =
                            (float)downloadedBytes * 100 / (initResponse.Content.Headers.ContentLength ?? 1L)
                        });
                        await memStream.WriteAsync(buffer);
                    }
                    bytes = memStream.ToArray();
                }
            }
            return bytes;

        }

        /// <summary>
        /// Notify interested objects that progress has been made 
        /// </summary>
        private void DownloadProgressChanged(object? sender, SingleDlProgressInfo e)
        {

        }

    }
}
