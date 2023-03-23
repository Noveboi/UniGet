using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    public class DownloadProgressModel
    {
        public string DownloadObject { get; set; }
        public string? StatusString { get; set; }
        public int DownloadsScheduled { get; set; }
        public int DownloadsCompleted { get; set; }
        public double PercentComplete
        {
            get => ((double)DownloadsCompleted / DownloadsScheduled) * 100;
        }

        public DownloadProgressModel(string DownloadObject)
        {
            this.DownloadObject = DownloadObject;
        }
    }
}
