using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    public class ScheduledProgressInfoEventArgs : EventArgs
    {
        public int TotalScheduled { get; set; }
        public ScheduledProgressInfoEventArgs(int TotalScheduled)
        {
            this.TotalScheduled = TotalScheduled;
        }
        public ScheduledProgressInfoEventArgs()
        {
            TotalScheduled = 0;
        }
    }
}
