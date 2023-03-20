using System.ComponentModel;

namespace University
{
    public enum DocType
    {
        [Description("Directory")]
        Dir,
        [Description("PDF")]
        Pdf,
        [Description("Docx")]
        Docx,
        [Description("ZIP")]
        Zip,
        [Description("MP4")]
        Mp4,
        [Description("Txt")]
        Txt,
        [Description("Py")]
        Py,
        [Description("Doc")]
        Doc,
        [Description("MP3")]
        Mp3,
        [Description("JPG")]
        Jpg,
        [Description("Jpeg")]
        Jpeg,
        [Description("PNG")]
        Png,
        [Description("Unknown Document")]
        Unknown
    }
}
