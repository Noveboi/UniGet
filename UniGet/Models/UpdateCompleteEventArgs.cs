using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using University;

namespace UniGet.Models
{
    public class UpdateCompleteEventArgs : EventArgs
    {
        public Dictionary<string, DocumentCollection> updateDocuments { get; } = new Dictionary<string, DocumentCollection>();
    }
}
