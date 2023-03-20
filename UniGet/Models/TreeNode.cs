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
    public class TreeNode
    {
        public string Name { get; }
        /// <summary>
        /// NULL if <see cref="TreeNode"/> is a <see cref="Course"/>
        /// </summary>
        public Subject Subject { get; set; }
        public ObservableCollection<TreeNode> Children { get; }

        public TreeNode(string Name)
        {
            this.Name = Name;
            this.Children = new ObservableCollection<TreeNode>();
        }
    }
}
