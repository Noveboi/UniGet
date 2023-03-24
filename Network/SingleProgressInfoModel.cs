using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    internal class SingleProgressInfoModel
    {
        public string Downloadee { get; }
        public long TotalBytes { get; }
        public long BytesDownloaded { get; set; }
        public double Percentage
        {
            get => (double)BytesDownloaded * 100/ TotalBytes ;
        }
        public bool DownloadComplete { get; set; }

        public SingleProgressInfoModel(string Downloadee, long TotalBytes)
        {
            this.Downloadee = Downloadee;
            this.TotalBytes = TotalBytes;
            DownloadComplete = false;
        }
    }
}
