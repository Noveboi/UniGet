using System.Diagnostics;
using University;
using Network;

namespace FileManagers
{
    internal class FileManager : IFileManager
    {
        Downloader downloader;
        public FileManager()
        {
            downloader = new();
        }

        public async Task DownloadDocumentAsync(Document document, string path)
        {
            string fullPath = GetFullFilePath(document, path);

            Debug.WriteLine($"Downloading document {document.Name}.");
            Stopwatch watch = Stopwatch.StartNew();

            byte[] bytes = await downloader.DownloadAsync(document.DownloadLink, document.Name);

            File.WriteAllBytes(fullPath, bytes);

            watch.Stop();
            Debug.WriteLine($"Finished downloading {document.Name} in {(double)watch.ElapsedMilliseconds / 1000}s");
        }
        public string GetFullFilePath(Document document, string cwd)
        {
            string filteredName = ReplaceIllegalChars(document.Name);
            string extension = GetDocType(document);
            if (cwd.Contains($"{filteredName}.{extension}"))
                return cwd;
            string fullPath = $"{cwd}/{filteredName}.{extension}";
            return fullPath;
        }
        public bool FileNeedsUpdate(string fullPath, DateTime datePublished)
        {
            if (File.Exists(fullPath))
            {
                Debug.WriteLine($"Found existing file at {fullPath}");
                if (File.GetCreationTime(fullPath) < datePublished)
                {
                    Debug.WriteLine($"Document at: {fullPath} requires update.");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"The existing file remains unchanged");
                    return false;
                }
            }
            return true;
        }

        private string ReplaceIllegalChars(string fileName)
        {
            if (fileName.Contains('/'))
            {
                fileName = fileName.Replace('/', '-');
                ReplaceIllegalChars(fileName);
            }
            if (fileName.Contains('\\'))
            {
                fileName = fileName.Replace('\\', '-');
                ReplaceIllegalChars(fileName);
            }
            if (fileName.Contains(':'))
            {
                fileName = fileName.Replace(':', ' ');
                ReplaceIllegalChars(fileName);
            }
            if (fileName.Contains('*'))
            {
                fileName = fileName.Replace('?', ' ');
                ReplaceIllegalChars(fileName);
            }
            if (fileName.Contains('"'))
            {
                fileName = fileName.Replace('"', '\'');
                ReplaceIllegalChars(fileName);
            }
            if (fileName.Contains('<'))
            {
                fileName = fileName.Replace('<', '(');
                ReplaceIllegalChars(fileName);
            }
            if (fileName.Contains('>'))
            {
                fileName = fileName.Replace('>', ')');
                ReplaceIllegalChars(fileName);
            }
            if (fileName.Contains('|'))
            {
                fileName = fileName.Replace('|', '-');
                ReplaceIllegalChars(fileName);
            }

            return fileName;
        }

        private string GetDocType(Document doc)
        {
            if (doc.Name.Contains('.'))
                return string.Empty;
            switch (doc.Type)
            {
                case DocType.Pdf:
                    return "pdf";
                case DocType.Docx:
                    return "docx";
                case DocType.Zip:
                    return "zip";
                case DocType.Mp4:
                    return "mp4";
                case DocType.Txt:
                    return "txt";
                case DocType.Py:
                    return "py";
                case DocType.Doc:
                    return "doc";
                case DocType.Mp3:
                    return "mp3";
                case DocType.Jpg:
                    return "jpg";
                case DocType.Jpeg:
                    return "jpeg";
                case DocType.Png:
                    return "png";
                case DocType.Unknown:
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }
    }
}
