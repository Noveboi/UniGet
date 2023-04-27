using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

namespace Network
{
    /// <summary>
    /// Provides methods to download content from the web using HTTP, additionally, it displays download progress
    /// </summary>
    public class ClientDownloader
    {
        private static HttpClientHandler _handler = new()
        {
            UseDefaultCredentials = true,
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 10,
            AutomaticDecompression = DecompressionMethods.GZip
        };
        private static HttpClient _client = new(_handler) { Timeout = Timeout.InfiniteTimeSpan };

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

        /// <summary>
        /// All HTTP requests are sent from here. 
        /// For each GET request, the response is read in the form of a Stream and additionally a SingleProgressInfo model is registered.
        /// </summary>
        /// <returns> The response's content byte array </returns>
        private static async Task<byte[]> DownloadAndReportAsync(Uri uri, string? fileName = null)
        {
            const int BUF_SIZE = 4096;
            long downloadedBytes = 0;
            string progId;

            // Initially get the headers of the page specified at URI, and read the Content-Length header 
            HttpResponseMessage response = await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

            long totalBytes = response.Content.Headers.ContentLength ?? -1L;

            // Register a new progress model for the page to be downloaded
            try
            {
                progId = uri.AbsoluteUri;
                ProgressReporter.RegisterDownloadProgress(uri.AbsoluteUri, totalBytes);
            }
            catch (ArgumentException)
            {
                progId = uri.GetHashCode().ToString();
                ProgressReporter.RegisterDownloadProgress(uri.AbsoluteUri, totalBytes, progId);
            }

            // Create a MemoryStream for writing the received data into a byte array
            using var memStream = new MemoryStream(BUF_SIZE);
            // Read the content of the response into a Stream
            using Stream responseStream = await response.Content.ReadAsStreamAsync();

            byte[] buffer = new byte[BUF_SIZE];
            long bytesRead = 0;

            // try/finally is for proper disposal of the responseStream
            try
            {
                // Read the received responseStream in chunks. This is for the purposes of reporting the progress made 
                // on the downloading
                while ((bytesRead = await responseStream.ReadAsync(buffer.AsMemory(0, BUF_SIZE))) > 0)
                {
                    downloadedBytes += bytesRead;
                    byte[] bufferCopy = new byte[bytesRead];
                    Buffer.BlockCopy(buffer, 0, bufferCopy, 0, (int)bytesRead);
                    await memStream.WriteAsync(bufferCopy);
                    ProgressReporter.ReportDownloadProgress(progId, downloadedBytes, false);
                }
            }
            finally
            {
                responseStream.Dispose();
            }
            response.Dispose();

            //bytes = await response.Content.ReadAsByteArrayAsync();

            ProgressReporter.ReportDownloadProgress(progId, downloadedBytes, true);
            return memStream.ToArray();
        }
    }
}
