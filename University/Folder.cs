using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University
{
    [Serializable]
    public class Folder : IUniversityFile
    {
        public DocType Type { get { return DocType.Dir; } }
        public string Name { get; set; }
        public long Size
        {
            get
            {
                long size = 0;
                Documents.ForEach(doc => size += doc.Size);
                return size;
            }
        }
        public DateTime Date { get; }
        public DocumentCollection Documents { get; }

        #region Constructors
        public Folder(string name, string date) 
        {
            Name = name;
            Date = DateTime.ParseExact(date, "dd-MM-yyyy HH:mm:ss", null);
            Documents = new DocumentCollection();
        }

        [JsonConstructor]
        public Folder(string name, long size, string date, DocumentCollection documents)
        {
            Name = name;
            Date = DateTime.ParseExact(date, "dd-MM-yyyy HH:mm:ss", null);
            Documents = documents;
        } 

        public Folder(string name)
        {
            Name = name;
            Date = DateTime.Now;
            Documents = new DocumentCollection();
        }

        public Folder(string name, DateTime date)
        {
            Name = name;
            Date = date;
            Documents = new DocumentCollection();
        }
        #endregion

        public static Folder Empty => new Folder("Empty");

        public string DownloadLink => string.Empty;

        public bool IsEmpty() => Documents.BaseCount == 0;
        public bool MetadataEquals(object obj)
        {
            return obj is Folder fol &&
                (this.Name == fol.Name && this.Date == fol.Date);
        }
        public override string ToString() => Name;

        public static bool operator ==(Folder a, Folder b)
        {
            if (a is null) return b is null;
            foreach (var doc in a.Documents.Folders)
            {
                if (!b.Documents.Folders.Contains(doc))
                    return false;
            }
            return true;
        }

        public static bool operator !=(Folder a, Folder b)
        {
            foreach (var doc in a.Documents.Folders)
            {
                if (!b.Documents.Folders.Contains(doc))
                    return true;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            return obj is Folder folder && folder == this;
        }

        public override int GetHashCode()
        {
            int hashCode = -656419763;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Date.GetHashCode();
            hashCode = hashCode * -1521134295 + Size.GetHashCode();
            return hashCode;
        }
    }
}
