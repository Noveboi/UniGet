using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace University
{
    [Serializable]
    public class Document : IUniversityFile
    {
        public DocType Type { get; } //enum
        public string Name { get; }
        public virtual long Size { get; } // expressed in Bytes
        public DateTime Date { get; }
        public string DownloadLink { get; }

        public DocumentCollection Documents => new DocumentCollection();

        /// <summary>
        /// Constructor used when reading scraped HTML
        /// </summary>
        public Document(string type, string name, string size, string date, string downloadLink)
        {
            Type = DetermineDocType(type);
            Name = name;
            Size = DetermineSize(size);
            Date = DateTime.ParseExact(date, "dd-MM-yyyy HH:mm:ss", null);
            DownloadLink = downloadLink;
        }

        [JsonConstructor]
        public Document(int type, string name, long size, string date, string downloadLink)
        {
            Type = (DocType)type;
            Name = name;
            Size = size;
            Date = DateTime.ParseExact(date, "dd-MM-yyyy HH:mm:ss", null);
            DownloadLink = downloadLink;
        }

        public Document(DocType type, string name, long size, DateTime date, string downloadLink)
        {
            Type = type;
            Name = name;
            Size = size;
            Date = date;
            DownloadLink = downloadLink;
        }

        private DocType DetermineDocType(string type)
        {
            switch (type)
            {
                case ".dir":
                    return DocType.Dir;
                case "pdf":
                    return DocType.Pdf;
                case "docx":
                    return DocType.Docx;
                case "zip":
                    return DocType.Zip;
                case "mp4":
                    return DocType.Mp4;
                case "txt":
                    return DocType.Txt;
                case "py":
                    return DocType.Py;
                case "doc":
                    return DocType.Doc;
                case "mp3":
                    return DocType.Mp3;
                case "jpg":
                    return DocType.Jpg;
                case "jpeg":
                    return DocType.Jpeg;
                case "png":
                    return DocType.Png;
                default:
                    return DocType.Unknown;
            }
        }

        private long DetermineSize(string size)
        {
            if (size == string.Empty || size == null || size == "&nbsp;")
                return 0;

            string[] splitStr = size.Split(' ');
            if (splitStr.Length == 1)
                return long.Parse(splitStr[0]);
            double multiplier = 1;

            double numericSize = double.Parse(splitStr[0]);
            switch (splitStr[1])
            {
                case "KB":
                    multiplier = 1024;
                    break;
                case "MB":
                    multiplier = 1048576;
                    break;
                case "GB":
                    multiplier = 1073741824;
                    break;
            }

            return (long)(multiplier * numericSize);
        }

        #region Override Methods
        public override string ToString()
        {
            return $"({Type}) - {Name} | {Math.Round((double)Size/1024, 2)} KB | {Date}";
        }

        public static bool operator ==(Document a, Document b)
        {
            return (a.Date == b.Date) && (a.Size == b.Size) && (a.Name == b.Name);
        }

        public static bool operator !=(Document a, Document b)
        {
            return (a.Date != b.Date) || (a.Size != b.Size) || (a.Name != b.Name);
        }

        public override bool Equals(object obj)
        {
            return obj is Document doc && doc == this;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}