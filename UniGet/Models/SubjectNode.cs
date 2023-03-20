using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniGet.ViewModels;
using University;

namespace UniGet.Models
{
    /// <summary>
    /// Used for the subject list, provides the necessary data to display info on the subject's documents and more...
    /// </summary>
    public class SubjectNode
    {
        /// <summary>
        /// Store the subject's document that are displayed on the main table when selected
        /// </summary>
        public ObservableCollection<DocumentDataGridModel>? Documents { get; set; }
        public Subject Subject { get; }

        public SubjectNode(Subject subject)
        {
            Subject = subject;
            Documents = DocumentDataGridModel.GetDocumentHierarchy(subject.Documents);
        }
    }
}
