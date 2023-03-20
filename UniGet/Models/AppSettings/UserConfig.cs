using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using University;

namespace UniGet.Models.AppSettings
{
    internal class UserConfig
    {
        public string ApplicationDirectory { get; set; }
        public List<Subject> Subscriptions { get; set; } 

        public UserConfig()
        {
            ApplicationDirectory = string.Empty;
            Subscriptions = new List<Subject>();
        }
    }
}
