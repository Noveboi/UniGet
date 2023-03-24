using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    public class MultiProgressInfoEventArgs
    {
        public List<string> DownloadingNames { get; } = new();
        public int OngoingProgCount { get; }

        public MultiProgressInfoEventArgs(List<string> DownloadingNames)
        {
            this.DownloadingNames = DownloadingNames;
            OngoingProgCount = DownloadingNames.Count;
        }
    }
}
