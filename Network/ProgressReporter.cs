using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    public static class ProgressReporter
    {
        private static readonly object _lockObject = new();
        private static readonly Dictionary<string, SingleProgressInfoModel> _progs = new();

        public static event EventHandler<SingleProgressInfoEventArgs>? ProgressChanged;
        public static event EventHandler<MultiProgressInfoEventArgs>? OngoingProgressAmountChanged;
        public static void RegisterProgress(string downloadee, long bytes, string? progressId = null)
        {
            var info = new SingleProgressInfoModel(downloadee, bytes);
            lock (_lockObject)
            {
                if (_progs.ContainsKey(downloadee) || (progressId != null && _progs.ContainsKey(progressId)))
                {
                    throw new ArgumentException($"Progress dictionary already contains key {progressId ?? downloadee}");
                }

                if (progressId != null)
                {
                    _progs.Add(progressId, info);
                }
                else
                {
                    _progs.Add(downloadee, info);
                }
            }
            OnOngoingProgressChanged(new MultiProgressInfoEventArgs(_progs.Keys.ToList()));
        }

        public static void ReportProgress(string progressId, long bytesDownloaded, bool downloadComplete)
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
                    prog = _progs[progressId];
                }
            }

            prog.BytesDownloaded = bytesDownloaded;
            prog.DownloadComplete = downloadComplete;
            OnProgressChanged(new SingleProgressInfoEventArgs(prog.Downloadee, prog.Percentage, downloadComplete));

            if (prog.BytesDownloaded == prog.TotalBytes || prog.DownloadComplete)
            {
                lock (_lockObject)
                {
                    _progs.Remove(progressId);
                    OnOngoingProgressChanged(new MultiProgressInfoEventArgs(_progs.Keys.ToList()));
                }
            }
        }

        private static void OnProgressChanged(SingleProgressInfoEventArgs e)
        {
            ProgressChanged?.Invoke(null, e);
        }

        private static void OnOngoingProgressChanged(MultiProgressInfoEventArgs e)
        {
            OngoingProgressAmountChanged?.Invoke(null, e);
        }
    }
}
