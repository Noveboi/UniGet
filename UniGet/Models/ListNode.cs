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
    public class ListNode
    {
        public string Name { get; }
        /// <summary>
        /// NULL if <see cref="ListNode"/> is a <see cref="Course"/>
        /// </summary>
        public Subject Subject { get; set; }
        public ObservableCollection<ListNode> Children { get; }

        public ListNode(string Name)
        {
            this.Name = Name;
            Children = new ObservableCollection<ListNode>();
        }
    }
}
