using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University
{
    internal interface ISubjectCollection
    {
        List<Subject> Subjects { get; }
        long Size { get; }
        Subject GetSubject(string id);
    }
}
