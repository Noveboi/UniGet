using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    public class SingleProgressInfoEventArgs : EventArgs
    {
        public string Downloadee { get; }
        public double Percentage { get; }
        public bool DownloadComplete { get; }

        public SingleProgressInfoEventArgs(string Downloadee, double Percentage, bool DownloadComplete)
        {
            this.Downloadee = Downloadee;
            this.Percentage = Percentage;
            this.DownloadComplete = DownloadComplete;
        }
    }
}
