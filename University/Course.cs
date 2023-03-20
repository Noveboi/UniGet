using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University
{
    [Serializable]
    public class Course : IUniversityObject
    {
        public string Name { get; }
        public long Size
        {
            get
            {
                long sz = 0;
                Subjects.ForEach(sub => sz += sub.Size);
                return sz;
            }
        }
        public List<Subject> Subjects { get; }

        public Course(string Name, List<Subject> Subjects)
        {
            this.Name = Name;
            this.Subjects = Subjects;
        }

        public Subject GetSubject(string id)
        {
            return Subjects.Find(sub => sub.ID == id);
        }
    }
}
