using System;
using System.Collections.Generic;

namespace University
{
    [Serializable]
    public class Subscriptions : ISubjectCollection
    {
        public List<Subject> Subjects { get; }

        public long Size
        {
            get
            {
                long sz = 0;
                Subjects.ForEach(sub => sz += sub.Size);
                return sz;
            }
        }

        public Subscriptions()
        {
            Subjects = new List<Subject>();
        }

        public void AddRange(IEnumerable<Subject> subjects)
        {
            foreach (var subject in subjects) 
                Add(subject);
        }

        public void Add(Subject subject)
        {
            if (!Subjects.Contains(subject))
                Subjects.Add(subject);
        }

        public Subject GetSubject(string id)
        {
            return Subjects.Find(sub => sub.ID == id);
        }
    }
}
