using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniGet.Models
{
    public static class Reporter
    {
        public static List<Progress<UniversalProgressDataModel>> Ongoing { get; } = new();
        
        /// <param name="prog">Data to report</param>
        /// <param name="handler">Bind the method you want to fire when progress changes</param>
        public static void AddNewProgress(UniversalProgressDataModel dataModel, Action<UniversalProgressDataModel> handler)
        {
            Progress<UniversalProgressDataModel> prog = new(handler);
            Ongoing.Add(prog);
        }
        
    }
}
