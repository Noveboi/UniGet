using FileManagers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniGet.Models
{
    /// <summary>
    /// Stores the course names found in GuNET's course catalogue on the disk
    /// </summary>
    [Serializable]
    internal class CourseNameGetModel
    {
        private static string _path = $"{Shared.ConfigDirectory}/course_names.json";
        public DateTime LastCheckDate { get; set; }
        public List<(string CourseName, string CourseID)> CourseInfo { get; set; }

        public CourseNameGetModel()
        {
            CourseInfo = new List<(string, string)>();
        }

        public static CourseNameGetModel Get()
        {
            try
            {
                JsonManager.ReadJsonFromFile<CourseNameGetModel>(_path);
            }
            catch (FileNotFoundException ex)
            {
                AppLogger.WriteLine($"Fixing exception: {ex.Message}", AppLogger.MessageType.HandledException);
                JsonManager.WriteJsonToFile(new CourseNameGetModel(), _path);
            }

            return JsonManager.ReadJsonFromFile<CourseNameGetModel>(_path);

        }
    }
}
