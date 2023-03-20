 using System;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Diagnostics;
using System.Linq;
using University;

namespace Scraper
{
    public static class UpdateChecker
    {
        /// <param name="before">The <see cref="Subject"/> before the most recent download</param>
        /// <param name="after">The <see cref="Subject"/> after the most recent download</param>
        /// <returns>The number of added/removed/modified documents</returns>
        public static DocumentCollection GetSubjectUpdates(Subject before, Subject after)
        {
            if (before != after)
                throw new Exception("The 'before' subject does not match the 'after' subject");
            Debug.WriteLine($"Checking for updates in subject: {before}");
            Stopwatch watch = Stopwatch.StartNew();

            DocumentCollection updates = GetFolderUpdates
                (before.Documents.Pack("Old Folder"), after.Documents.Pack("New Folder"), new DocumentCollection());

            watch.Stop();
            Debug.WriteLine($"{updates.BaseCount} updates found! Searched in {watch.ElapsedMilliseconds}ms");
            return updates;
        }

        /// <param name="oldFolder">The <see cref="Folder"/> before the most recent download</param>
        /// <param name="newFolder">The <see cref="Folder"/> after the most recent download</param>
        /// <returns>The number of added/removed/modified documents</returns>
        private static DocumentCollection GetFolderUpdates(Folder oldFolder, Folder newFolder, DocumentCollection documents)
        {
            // DETECTS: Addition, Modification
            // Iterate through each of the new folder's documents
            // and compare them to the old folder's documents 
            foreach (var doc in newFolder.Documents.Files)
            {
                // Detect if new document has been added
                if (!oldFolder.Documents.Files.Exists(oldDoc => oldDoc.Name == doc.Name))
                {
                    Debug.WriteLine($"\t\tDetected added document: {doc}");
                    documents.Files.Add(doc);
                }
                // Detect if existing document has been modified
                if (oldFolder.Documents.Files.Exists
                    (oldDoc => oldDoc.Name == doc.Name && oldDoc.Date != oldDoc.Date))
                {
                    Debug.WriteLine($"\t\tDetected modified document: {doc}");
                    documents.Files.Add(doc);
                }   
            }
            // DETECTS: Folder addition
            // Iterate through each of the new folder's folders
            // and compare them to the old folder's folders, if the folders match, then 
            // recursively search the folder that matches
            foreach (var folder in newFolder.Documents.Folders)
            {
                Folder? old;
                if ((old = oldFolder.Documents.Folders.Find(fol => fol.MetadataEquals(folder))) is not null )
                {
                    Debug.WriteLine($"\tSearching in {old.Name}");
                    GetFolderUpdates(old, folder, documents);
                }
                else
                {
                    Debug.WriteLine($"\t\tDetected added folder: {folder}");
                    GetFolderUpdates(Folder.Empty, folder, documents);
                }
            }
            // DETECTS: Removal
            // Iterate through each of the old folder's documents
            // and compare them to the new folder's documents
            foreach (var doc in oldFolder.Documents.Files)
            {
                if (!newFolder.Documents.Files.Exists(newDoc => newDoc.Name == doc.Name))
                {
                    Debug.WriteLine($"\t\tDetected removed document: {doc}");
                }
            }
            // DETECTS: Removal
            // Iterates through each of the old folder's folders
            // and compare them to the new folder's folders
            foreach (var folder in oldFolder.Documents.Folders)
            {
                if (!newFolder.Documents.Folders.Exists(fol => fol.MetadataEquals(folder)))
                {
                    Debug.WriteLine($"\t\tDetected removed folder: {folder}");
                }
            }

            return documents;
        }
    }
}
