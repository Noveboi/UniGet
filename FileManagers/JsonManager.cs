using System.Text;
using Newtonsoft.Json;
using University;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Runtime.Intrinsics.X86;

namespace FileManagers
{
    /// <summary>
    /// Serializes and deserializes to and from JSON files 
    /// </summary>
    public static class JsonManager
    {
        /// <summary>
        /// Serialize the given <see cref="Course"/> object to a file named (<paramref name="course"/>.Name).json
        /// </summary>
        /// <param name="course"></param>
        public static void WriteCourseToFile(Course course)
        {
            WriteJsonToFile(course, $"{Shared.ConfigDirectory}/{course.Name}.json");
        }
        /// <summary>
        /// Deserialize the JSON file with name <paramref name="courseName"/>.json to a Course object.
        /// If the course file is empty, the method returns null.
        /// </summary>
        /// <param name="courseName">The file name to search</param>
        /// <returns>The deserialized course object</returns>
        public static Course? ReadCourseFromFile(string courseName)
        {
            return ReadJsonFromFile<Course>($"{Shared.ConfigDirectory}/{courseName}.json");
        }

        /// <summary>
        /// Deserialize the JSON file from path <paramref name="filePath"/> to an object of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Specifies the type of object to deserialize the JSON string to</typeparam>
        /// <returns>The deserialized object</returns>
        public static T? ReadJsonFromFile<T>(string filePath)
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

        /// <summary>
        /// Serialize the given object <paramref name="serializableObj"/> of type <typeparamref name="T"/> to a JSON string
        /// and write it to the path given by <paramref name="filePath"/>
        /// </summary>
        /// <typeparam name="T">Specifies the type of the object that is to be serialized</typeparam>
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
