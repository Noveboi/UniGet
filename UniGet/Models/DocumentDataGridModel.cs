using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UniGet.ViewModels;
using University;

namespace UniGet.Models
{
    public class DocumentDataGridModel : INotifyPropertyChanged
    {
        private long _size;
        private static DocumentDataGridModel _dummyModelInstance = new DocumentDataGridModel();

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Provides the path to a filetype image, depending on the type of the document
        /// </summary>
        /// 
        public string TypeImagePath { get; }
        public string Name { get; }
        public string Size
        {
            get
            {
                if (_size >= 1024 && _size < 1048576)
                {
                    return $"{Math.Round((double)_size / 1024, 2)} KB";
                }
                else if (_size >= 1048576 && _size < 1073741824)
                {
                    return $"{Math.Round((double)_size / 1048576, 2)} MB";
                }
                else if (_size >= 1048576 && _size < 1099511627776)
                {
                    return $"{Math.Round((double)_size / 1099511627776, 2)} GB";
                }
                else if (_size >= 1099511627776)
                {
                    return $"{Math.Round((double)_size / 1099511627776, 2)} TB";
                }
                else
                {
                    return $"{_size} B";
                }
            }
        }
        public DateTime DatePublished { get; }
        /// <summary>
        /// Used for redirecting to another table containing the 
        /// <see cref="ObservableCollection{DocumentDataGridModel}"/> of the selected folder
        /// </summary>
        public ObservableCollection<DocumentDataGridModel>? Docs { get; }

        public IUniversityFile File { get; }

        public DocumentDataGridModel(DocType Type, string Name, long Size, DateTime DatePublished, DocumentCollection? Documents = null, string? DownloadLink = null)
        {
            TypeImagePath = GetFileImage(Type);
            this.Name = Name;
            _size = Size;
            this.DatePublished = DatePublished;

            Docs = GetDocumentHierarchy(Documents);
        }
        public DocumentDataGridModel(IUniversityFile file)
        {
            File = file;
            TypeImagePath = GetFileImage(file.Type);
            Name = file.Name;
            _size = file.Size;
            DatePublished = file.Date;
            Docs = GetDocumentHierarchy(file.Documents);
        }

        private DocumentDataGridModel() : this(DocType.Unknown, "Dummy", 100, DateTime.Now) { }
        public static DocumentDataGridModel Dummy => _dummyModelInstance;

        private string GetFileImage(DocType Type)
        {
            switch (Type)
            {
                case DocType.Dir:
                    return "Assets/folder-icon.png";
                case DocType.Pdf:
                    return "Assets/pdf-icon.png";
                case DocType.Doc:
                    return "Assets/doc-icon.png";
                case DocType.Docx:
                    return "Assets/docx-icon.png";
                case DocType.Py:
                    return "Assets/py-icon.png";
                case DocType.Mp3:
                    return "Assets/mp3-icon.png";
                case DocType.Mp4:
                    return "Assets/video-icon.png";
                case DocType.Txt:
                    return "Assets/txt-icon.png";
                case DocType.Zip:
                    return "Assets/zip-icon.png";
                case DocType.Jpg or DocType.Jpeg or DocType.Png:
                    return "Assets/image-icon.png";
                default:
                    return "Assets/avalonia-logo.ico";
            }
        }

        public static ObservableCollection<DocumentDataGridModel>? GetDocumentHierarchy(DocumentCollection? docs)
        {
            if (docs == null) 
                return null;
            var docCollection = new ObservableCollection<DocumentDataGridModel>();

            foreach(var folder in docs.Folders)
            {
                docCollection.Add
                    (new DocumentDataGridModel(folder));
            }
            foreach(var file in docs.Files)
            {
                docCollection.Add
                    (new DocumentDataGridModel(file));
            }

            return docCollection;
        }
    }
}
