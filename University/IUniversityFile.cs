﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University
{
    public interface IUniversityFile : IUniversityObject
    {
        DocType Type { get; }
        DateTime Date { get; }
        DocumentCollection Documents { get; }

        string DownloadLink { get; }
    }
}
