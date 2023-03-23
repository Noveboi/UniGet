using System.Text;
using Newtonsoft.Json;
using University;
using System.Diagnostics;

namespace FileManagers
{
    /// <summary>
    /// Reads and writes to JSON files. Configured to work for this app
    /// </summary>
    public static class JsonManager
    {
        private static string _configFilePath = $"{Shared.ApplicationDirectory}/config.json";
        public static void WriteCourseToFile(Course course)
        {
            WriteJsonToFile(course, $"{Shared.ConfigDirectory}/{course.Name}.json");
        }

        public static void WriteSubscriptionsToFile(Subscriptions subs)
        {
            WriteJsonToFile(subs, _configFilePath);
        }

        public static Course ReadCourseFromFile(string courseName)
        {
            return ReadJsonFromFile<Course>($"{Shared.ConfigDirectory}/{courseName}.json");
        }

        public static Subscriptions ReadSubscriptionsFromFile()
        {
            return ReadJsonFromFile<Subscriptions>(_configFilePath);
        }

        public static T ReadJsonFromFile<T>(string filePath)
        {
            Stopwatch watch = Stopwatch.StartNew();
            AppLogger.WriteLine($"Starting reading JSON from {filePath}.");

            JsonSerializer serializer = new JsonSerializer();
            SetSerializerProperties(serializer);

            T? obj;

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }

            using (StreamReader reader = new StreamReader
                (new FileStream(filePath, FileMode.Open), Encoding.UTF8))
            using (JsonReader jsonReader = new JsonTextReader(reader))
            {
                obj = serializer.Deserialize<T>(jsonReader);
            }

            watch.Stop();
            AppLogger.WriteLine($"Finished reading JSON from {filePath}. Took {watch.ElapsedMilliseconds}ms");

            return obj;
        }

        public static void WriteJsonToFile<T>(T serializableObj, string filePath)
        {
            Stopwatch watch = Stopwatch.StartNew();
            AppLogger.WriteLine($"Starting writing JSON object: {serializableObj} to {filePath}.");

            JsonSerializer serializer = new JsonSerializer();
            SetSerializerProperties(serializer);

            using (StreamWriter writer = new StreamWriter
                (new FileStream(filePath, FileMode.Create), Encoding.UTF8))
            using (JsonWriter jsonWriter = new JsonTextWriter(writer))
            {
                serializer.Serialize(jsonWriter, serializableObj);
            }

            watch.Stop();
            AppLogger.WriteLine($"Finished writing to JSON object at {filePath}. Took {watch.ElapsedMilliseconds}ms");
        }

        private static void SetSerializerProperties(JsonSerializer serializer)
        {
            serializer.DateFormatString = "dd-MM-yyyy HH:mm:ss";
            serializer.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
        }
    }
}
