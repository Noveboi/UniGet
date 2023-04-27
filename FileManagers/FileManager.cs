using University;
using Network;

namespace FileManagers
{
    /// <summary>
    /// Downloads specific <see cref="Document"/>s or entire <see cref="Subject"/>s into disk
    /// </summary>
    public class FileManager
    {
        private readonly IFileDownloader _fileDownloader;
        public FileManager()
        {
            _fileDownloader = new FileDownloader();
        }

        #region Download Methods (Write)
        /// <summary>
        /// Download the subject's documents OR the subject's specified documents into disk, creating the necessary folders along the way
        /// </summary>
        /// <param name="subject">The subject to download to disk</param>
        /// <param name="specificDocuments">Overwrites the download process and only download the documents that are specified in the collection</param>
        public async Task DownloadSubjectAsync(Subject subject, DocumentCollection? specificDocuments = null)
        {
            var directoryStack = new DirectoryStack(DirectoryStack.Mode.Write);

            // Kickstart the process by pushing the main subject folder into the DirectoryStack
            directoryStack.Push(subject.Name, subject.Documents);
            string fullPath = directoryStack.GetCwd();
            Directory.CreateDirectory(fullPath);

            if (specificDocuments == null)
                await GetDocumentsAsync(subject.Documents, directoryStack);
            else
                await GetDocumentsAsync(subject.Documents, directoryStack, specificDocuments);
        }

        /// <summary>
        /// Download the files specified in <paramref name="documents"/> OR download the files specified in <paramref name="specificDocuments"/>.
        /// The method downloads files ONLY if the file needs an update
        /// </summary>
        /// <param name="documents"> The collection of documents to download </param>
        /// <param name="directoryStack"> Passed recursively to other GetDocumentsAsync calls in order to preserve the directory structure </param>
        /// <param name="specificDocuments"> 
        /// A subset of <paramref name="documents"/>, that overwrites the original <paramref name="documents"/>
        /// collection. If not null, the files in <paramref name="specificDocuments"/> are downloaded ONLY.
        /// </param>
        /// <returns></returns>
        private async Task GetDocumentsAsync(DocumentCollection documents, DirectoryStack directoryStack, DocumentCollection? specificDocuments = null)
        {
            // 0.5 -> Determine the required downloads for progress reporting
            int downloadsScheduled = 0;

            foreach(Document file in documents.Files) 
            {
                string fullPath = _fileDownloader.GetFullFilePath(file, directoryStack.GetCwd());

                if ((specificDocuments != null
                    && specificDocuments.Files.Contains(file))
                    || specificDocuments == null)
                {
                    if (_fileDownloader.FileNeedsUpdate(fullPath, file.Date))
                        downloadsScheduled++;
                }
            }


            // 1 -> Download all the documents and then move on to Folders

            foreach (Document file in documents.Files)
            {
                string dirPath = directoryStack.GetCwd();
                string fullPath = _fileDownloader.GetFullFilePath(file, dirPath);

                if ((specificDocuments != null
                    && specificDocuments.Files.Contains(file))
                    || specificDocuments == null)
                {
                    if (_fileDownloader.FileNeedsUpdate(fullPath, file.Date))
                    {
                        // Create the directory if it doesn't exist
                        Directory.CreateDirectory(dirPath);
                        await _fileDownloader.DownloadDocumentAsync(file, fullPath);
                    }
                }
            }

            // 2 -> When all documents have been downloaded, begin navigating the folders

            foreach (Folder folder in documents.Folders)
            {
                directoryStack.Push(folder.Name, folder.Documents);
                await GetDocumentsAsync(folder.Documents, directoryStack, specificDocuments);
                directoryStack.Pop();
            }
        }
        #endregion
    }
}
