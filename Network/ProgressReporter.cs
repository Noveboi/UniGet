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
    /// <item> 
    /// <see cref="SingleProgressInfoModel"/> is used for keeping track of how many bytes have been downloaded from a specific file. 
    /// The progress reporter automatically registers another event of type <see cref="MultiProgressInfoEventArgs"/> that keeps 
    /// track of how many <see cref="SingleProgressInfoModel"></see> objects are still ongoing.
    /// </item>
    /// <item>
    /// <see cref="ScheduledProgressInfoEventArgs"/> is used for keeping track of the total amount of tasks that are to be done for any 
    /// action such as downloading. It is a simple counter, containing only the number of tasks and nothing else
    /// </item>
    /// </list>
    /// </summary>
    public static class ProgressReporter
    {
        private static readonly object _lockObject = new();
        private static readonly object _scheduleLock = new();
        private static bool _scheduleProgOverride = false;
        private static readonly Dictionary<string, SingleProgressInfoModel> _downloadProgs = new();
        private static ScheduledProgressInfoEventArgs _scheduledProgs = new();

        public static event EventHandler<SingleProgressInfoEventArgs>? DownloadProgressChanged;
        public static event EventHandler<MultiProgressInfoEventArgs>? OngoingProgressAmountChanged;
        public static event EventHandler<ScheduledProgressInfoEventArgs>? ScheduledProgressChanged;

        /// <summary>
        /// Add <paramref name="taskCount"/> scheduled tasks to the <see cref="ScheduledProgressInfoEventArgs"/> counter.
        /// </summary>
        public static void ScheduleProgress(int taskCount)
        {
            lock (_scheduleLock)
            {
                _scheduleProgOverride = true;
                _scheduledProgs.TotalScheduled += taskCount;
            }
            ScheduledProgressChanged?.Invoke(null, _scheduledProgs);
        }

        /// <summary>
        /// Decrement the <see cref="ScheduledProgressInfoEventArgs"/> counter by 1
        /// </summary>
        public static void FinishProgress()
        {
            lock (_scheduleLock)
            {
                _scheduledProgs.TotalScheduled--;
            }
            ScheduledProgressChanged?.Invoke(null, _scheduledProgs);

            if (_scheduledProgs.TotalScheduled == 0)
                _scheduleProgOverride = false;
        } 
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
            if (_scheduleProgOverride) return;

            OngoingProgressAmountChanged?.Invoke(null, e);
        }
    }
}
