using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University
{
    /// <summary>
    /// Represents object such as Courses, Subjects and Documents that share a name and a file size
    /// </summary>
    public interface IUniversityObject
    {
        string Name { get; }
        long Size { get; }
    }
}
