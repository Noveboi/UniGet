using FileManagers;
using System;
using System.Collections.Generic;
using System.IO;
using University;

namespace UniGet.Models.AppSettings
{
    /// <summary>
    /// Provides crucial info for the application such as user subscriptions, last update time, and more.
    /// The singleton is stored on disk in appsettings.json
    /// </summary>
    internal class LocalAppSettings
    {
        private const string _path = $"{Shared.ConfigDirectory}/appsettings.json";
        private static LocalAppSettings? _instance;

        public static LocalAppSettings GetInstance()
        {
            try
            {
                // Set the _instance ONLY when it's null
                _instance ??= JsonManager.ReadJsonFromFile<LocalAppSettings>(_path);
                if (_instance == null)
                    WriteDefaults();
            }
            catch (DirectoryNotFoundException dex)
            {
                AppLogger.WriteLine($"Fixing exception: {dex.Message}", AppLogger.MessageType.HandledException);
                Directory.CreateDirectory(Shared.ConfigDirectory);
                WriteDefaults();
            }
            catch (FileNotFoundException fex)
            {
                // Create the appsettings.json file using default values
                AppLogger.WriteLine($"Fixing exception: {fex.Message}", AppLogger.MessageType.HandledException);
                WriteDefaults();
            }

            if (_instance == null)
            {
                throw new Exception("LocalAppSettings instance is null after null-checking");
            }

            return _instance;
        }

        private LocalAppSettings()
        {
            UserConfig = new UserConfig();
            UserStats = new UserStats();
        }

        /// <summary>
        /// Add the <see cref="List{T} string"/> to the appsettings and write changes
        /// </summary>
        /// <param name="subjectIds"></param>
        public void AddSubscription(Subject subject)
        {
            if (!UserConfig.Subscriptions.Contains(subject))
            {
                UserConfig.Subscriptions.Add(subject);
                JsonManager.WriteJsonToFile(_instance, _path);
            }
        }

        /// <summary>
        /// Remove a selected subject from the appsettings and write changes
        /// </summary>
        /// <param name="subject"></param>
        public List<Subject> RemoveSubscription(Subject subject)
        {
            UserConfig.Subscriptions.Remove(subject);
            JsonManager.WriteJsonToFile(_instance, _path);
            return UserConfig.Subscriptions;
                
        }

        public void SetNewAppDirectory(string path)
        {
            UserConfig.ApplicationDirectory = path;
            JsonManager.WriteJsonToFile(_instance, _path);
        }

        public void SaveSettings()
        {
            JsonManager.WriteJsonToFile(_instance, _path);
        }

        /// <summary>
        /// Save default settings to appsettings.json
        /// </summary>
        public static void WriteDefaults()
        {
            _instance = new()
            {
                UserStats = new UserStats() { LastRunTime = DateTime.MinValue, LastUpdateTime = DateTime.MinValue },
                UserConfig = new UserConfig() { Subscriptions = new List<Subject>() }
            };

            JsonManager.WriteJsonToFile(_instance, _path);
        }

        public UserStats UserStats { get; set; }
        public UserConfig UserConfig { get; set; }
    }
}
