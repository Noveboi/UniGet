using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace University
{
    [Serializable]
    public class DocumentCollection
    {
        public long Size
        {
            get
            {
                long size = 0;
                Folders.ForEach(folder => size += folder.Size);
                Files.ForEach(file => size += file.Size);
                return size;
            }
        }
        public List<Folder> Folders { get; set; }
        public List<Document> Files { get; set; }

        /// <summary>
        /// Gets the count of the files and folders WITHOUT counting the documents contained in those folders
        /// </summary>
        [JsonIgnore]
        public int BaseCount => Files.Count + Folders.Count;
        /// <summary>
        /// Gets the count of the files and folders recursively 
        /// </summary>
        public int FullCount() => Count(this);

        public DocumentCollection()
        {
            Folders = new List<Folder>();
            Files = new List<Document>();
        }

        private int Count(DocumentCollection docs)
        {
            int count = docs.Files.Count;
            for (int i = 0; i < docs.Folders.Count; i++)
            {
                count += Count(docs.Folders[i].Documents);
            }
            return count;
        }

        public List<IUniversityFile> GetDocuments()
        {
            List<IUniversityFile> docs = new List<IUniversityFile>();
            docs.AddRange(Folders);
            docs.AddRange(Files);
            return docs;
        }

        public Folder Pack(string folderName)
        {
            Folder outFolder = new Folder(folderName);
            outFolder.Documents.Files.AddRange(Files);
            outFolder.Documents.Folders.AddRange(Folders);
            return outFolder;
        }

        public void ForEach(Action<IUniversityFile> action)
        {
            foreach(var doc in GetDocuments()) 
                action(doc);
        }

        public IEnumerable<IUniversityFile> GetEnumerator()
        {
            foreach(var doc in GetDocuments())
                yield return doc;
        }

    }
}
