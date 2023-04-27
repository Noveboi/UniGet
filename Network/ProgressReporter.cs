using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    /// <summary>
    /// Register and report various progress models in order to keep track of various asynchronous tasks happening.
    /// Currently supports the following progress models:
    /// <list type="bullet">
    /// <item> <see cref="SingleProgressInfoModel"/> is used for keep track of how many bytes have been downloaded from a specific file. 
    ///        The progress reporter automatically registers another event of type <see cref="MultiProgressInfoEventArgs"/> that keeps 
    ///        track of how many <see cref="SingleProgressInfoModel"></see> objects are still ongoing.
    /// </item>
    /// <item>
    /// Temp
    /// </item>
    /// </list>
    /// </summary>
    public static class ProgressReporter
    {
        private static readonly object _lockObject = new();
        private static readonly Dictionary<string, SingleProgressInfoModel> _downloadProgs = new();

        public static event EventHandler<SingleProgressInfoEventArgs>? DownloadProgressChanged;
        public static event EventHandler<MultiProgressInfoEventArgs>? OngoingProgressAmountChanged;
        /// <summary>
        /// Register a SingleProgressInfoModel to keep track of the downloading progress of a file/website
        /// </summary>
        /// <exception cref="ArgumentException">Is thrown when there is a duplicate key added in the progress dictionary</exception>
        public static void RegisterDownloadProgress(string downloadeeName, long bytes, string? progressId = null)
        {
            var info = new SingleProgressInfoModel(downloadeeName, bytes);
            lock (_lockObject)
            {
                if (_downloadProgs.ContainsKey(downloadeeName) || (progressId != null && _downloadProgs.ContainsKey(progressId)))
                {
                    throw new ArgumentException($"Progress dictionary already contains key {progressId ?? downloadeeName}");
                }

                if (progressId != null)
                {
                    _downloadProgs.Add(progressId, info);
                }
                else
                {
                    _downloadProgs.Add(downloadeeName, info);
                }
            }
            OnOngoingProgressChanged(new MultiProgressInfoEventArgs(_downloadProgs.Keys.ToList()));
        }

        /// <summary>
        /// Report a change in progress by specifying new updated values for an already existing SingleProgressInfoModel.
        /// </summary>
        public static void ReportDownloadProgress(string progressId, long bytesDownloaded, bool downloadComplete)
        {
            SingleProgressInfoModel prog;
            lock (_lockObject) 
            {
                if (downloadComplete)
                {
                    prog = new SingleProgressInfoModel(progressId, 1) { BytesDownloaded = 1, DownloadComplete = true };
                }
                else
                {
                    prog = _downloadProgs[progressId];
                }
            }

            prog.BytesDownloaded = bytesDownloaded;
            prog.DownloadComplete = downloadComplete;
            OnProgressChanged(new SingleProgressInfoEventArgs(prog.Downloadee, prog.Percentage, downloadComplete));

            if (prog.BytesDownloaded == prog.TotalBytes || prog.DownloadComplete)
            {
                lock (_lockObject)
                {
                    _downloadProgs.Remove(progressId);
                    OnOngoingProgressChanged(new MultiProgressInfoEventArgs(_downloadProgs.Keys.ToList()));
                }
            }
        }

        private static void OnProgressChanged(SingleProgressInfoEventArgs e)
        {
            DownloadProgressChanged?.Invoke(null, e);
        }

        private static void OnOngoingProgressChanged(MultiProgressInfoEventArgs e)
        {
            OngoingProgressAmountChanged?.Invoke(null, e);
        }
    }
}
