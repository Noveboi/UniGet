using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniGet.Models
{
    public class UniversalProgressDataModel
    {
        public string ObjectName { get; set; }
        public string? StatusString { get; set; }
        public int DownloadsScheduled { get; set; }
        public int DownloadsCompleted { get; set; }
        public double PercentComplete
        {
            get => ((double)DownloadsCompleted / DownloadsScheduled) * 100;
        }

        public UniversalProgressDataModel(string ObjectName)
        {
            this.ObjectName = ObjectName;
        }
    }
}
