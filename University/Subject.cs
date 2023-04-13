using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace University
{
    [Serializable]
    public class Subject : IUniversityObject
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public bool Locked { get; set; }
        public long Size
        {
            get
            {
                long sz = 0;
                Documents.ForEach(doc => sz += doc.Size);
                return sz;
            }
        }
        public string SiteLink { get; set; }
        public DocumentCollection Documents { get; set; }

        public Subject() => Documents = new DocumentCollection();
        
        /// <summary>
        /// Creates a copy of the <see cref="Subject"/> passed as an argument
        /// </summary>
        public Subject(Subject subjectToCopy)
        {
            Name = subjectToCopy.Name;
            ID = subjectToCopy.ID;
            Locked = subjectToCopy.Locked;
            SiteLink = subjectToCopy.SiteLink;
            Documents = subjectToCopy.Documents;
        }

        public override string ToString()
        {
            return $"{Name} ({ID}) {(Locked ? "LOCKED" : string.Empty)}";
        }

        public override bool Equals(object obj)
        {
            return obj is Subject subject &&
                   Name == subject.Name &&
                   ID == subject.ID;
        }

        public override int GetHashCode()
        {
            int hashCode = 1997165910;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ID);
            return hashCode;
        }

        public static bool operator ==(Subject a, Subject b) =>
            a.Name == b.Name && a.ID == b.ID;

        public static bool operator !=(Subject a, Subject b) =>
            a.Name != b.Name || a.ID != b.ID;

    }
}
