using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using University;

namespace FileManagers
{
    internal interface IFileDownloader
    {
        /// <summary>
        /// Downloads the specified document to disk.
        /// </summary>
        Task DownloadDocumentAsync(Document document, string path);

        /// <summary>
        /// Concatenates the <see cref="Document"/>'s name to the current working directory of the file manager
        /// </summary>
        /// <param name="cwd"> The directory to append the file name to</param>
        bool FileNeedsUpdate(string fullPath, DateTime datePublished);

        /// <summary>
        /// Compares the file on disk (if it exists) and the same file on GUNeT.
        /// It is then determined if the file requires a download
        /// </summary>
        /// <param name="fullPath">The full path for the file on disk</param>
        /// <param name="datePublished">The <see cref="DateTime"/> that the file from GUNeT was uploaded</param>
        string GetFullFilePath(Document document, string cwd);
    }
}
