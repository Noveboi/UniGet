using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    public static class Reporter
    {
        public static List<Progress<DownloadProgressModel>> Ongoing { get; } = new();
        
        /// <param name="prog">Data to report</param>
        /// <param name="handler">Bind the method you want to fire when progress changes</param>
        public static void AddNewProgress(DownloadProgressModel dataModel, Action<DownloadProgressModel> handler)
        {
            var prog = new Progress<DownloadProgressModel>(handler);
            Ongoing.Add(prog);
        }
        
    }
}
