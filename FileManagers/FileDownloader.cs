using University;
using Network;

namespace FileManagers
{
    /// <summary>
    /// Downloads specific <see cref="Document"/>s or entire <see cref="Subject"/>s into disk
    /// </summary>
    public class FileDownloader
    {
        private IFileManager _fileManager;
        public FileDownloader()
        {
            _fileManager = new FileManager();
            // Read from config.json into _initialDirectory 
        }

        #region Download Methods (Write)
        public async Task DownloadSubjectAsync(Subject subject, DocumentCollection? specificDocuments = null)
        {
            DirectoryStack directoryStack = new DirectoryStack(DirectoryStack.Mode.Write);
            Progress<MultiDlProgressInfo> progress = new Progress<MultiDlProgressInfo>();
            progress.ProgressChanged += MultiDownloadProgressChanged;

            // Kickstart the process by pushing the main subject folder into the DirectoryStack
            directoryStack.Push(subject.Name, subject.Documents);

            if (specificDocuments == null)
                await GetDocumentsAsync(subject.Documents, directoryStack, progress);
            else
                await GetDocumentsAsync(subject.Documents, directoryStack, progress, specificDocuments);
        }

        private async Task GetDocumentsAsync(DocumentCollection documents, DirectoryStack directoryStack, IProgress<MultiDlProgressInfo> progress, DocumentCollection? specificDocuments = null)
        {
            // 0.5 -> Determine the required downloads for progress reporting
            int downloadsScheduled = 0;

            foreach(Document file in documents.Files) 
            {
                string fullPath = _fileManager.GetFullFilePath(file, directoryStack.GetCwd());

                if ((specificDocuments != null
                    && specificDocuments.Files.Contains(file))
                    || specificDocuments == null)
                {
                    if (_fileManager.FileNeedsUpdate(fullPath, file.Date))
                        downloadsScheduled++;
                }
            }

            var progInfo = new MultiDlProgressInfo() 
            { DownloadsScheduled = downloadsScheduled, CompletedDownloads = 0 };

            // 1 -> Download all the documents and then move on to Folders

            foreach (Document file in documents.Files)
            {
                string fullPath = _fileManager.GetFullFilePath(file, directoryStack.GetCwd());

                if ((specificDocuments != null
                    && specificDocuments.Files.Contains(file))
                    || specificDocuments == null)
                {
                    if (_fileManager.FileNeedsUpdate(fullPath, file.Date))
                        await _fileManager.DownloadDocumentAsync(file, fullPath);
                    progInfo.CompletedDownloads++;

                    progress?.Report(progInfo);
                }
            }

            // 2 -> When all documents have been downloaded, begin navigating the folders

            foreach (Folder folder in documents.Folders)
            {
                directoryStack.Push(folder.Name, folder.Documents);
                await GetDocumentsAsync(folder.Documents, directoryStack, progress, specificDocuments);
                directoryStack.Pop();
            }
        }
        #endregion
        /// <summary>
        /// Report progress to interested objects
        /// </summary>
        private void MultiDownloadProgressChanged(object? sender, MultiDlProgressInfo e)
        {
        }

    }
}
