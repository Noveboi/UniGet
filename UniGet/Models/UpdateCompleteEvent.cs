using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using University;

namespace UniGet.Models
{
    public class UpdateCompleteEvent
    {
        public Dictionary<Subject, DocumentCollection> updateDocuments { get; }

        public UpdateCompleteEvent(Dictionary<Subject, DocumentCollection> updateDocuments)
        {
            this.updateDocuments = updateDocuments;
        }
    }
}
