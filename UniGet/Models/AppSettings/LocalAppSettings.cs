using Avalonia;
using Avalonia.Controls;
using FileManagers;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using University;

namespace UniGet.Models.AppSettings
{
    internal class LocalAppSettings
    {
        private static string _path = $"{Shared.ConfigDirectory}/appsettings.json";
        private static LocalAppSettings _instance;

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

        public void SaveSettings()
        {
            JsonManager.WriteJsonToFile(_instance, _path);
        }

        /// <summary>
        /// Save default settings to appsettings.json
        /// </summary>
        public static void WriteDefaults()
        {
            _instance = new();
            _instance.UserStats = new UserStats() { LastRunTime = DateTime.MinValue, LastUpdateTime = DateTime.MinValue };
            _instance.UserConfig = new UserConfig() { Subscriptions = new List<Subject>() };
            JsonManager.WriteJsonToFile(_instance, _path);
        }

        public UserStats UserStats { get; set; }
        public UserConfig UserConfig { get; set; }
    }
}
