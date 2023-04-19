using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using FileManagers;
using Scraper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UniGet.Models;
using UniGet.Models.AppSettings;
using UniGet.ViewModels;
using UniGet.Views;
using University;

namespace UniGet
{
    public partial class App : Application
    {
        public event EventHandler<UpdateCompleteEvent>? UpdateComplete;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Execute update checking and downloading asynchronously, continue working on building and initializing app
                new Action(async () =>
                {
                    await DoBeforeAppInitAsync();
                })();

                desktop.Exit += Desktop_Exit;

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }
            base.OnFrameworkInitializationCompleted();
        }

        private async Task DoBeforeAppInitAsync()
        {
            if (!Directory.Exists(Shared.ConfigDirectory))
                Directory.CreateDirectory(Shared.ConfigDirectory);

            AppLogger.ClearLog();
            await AppLogger.WriteLineAsync($"APPLICATION LAUNCHED");

            Shared.ApplicationDirectory = LocalAppSettings.GetInstance().UserConfig.ApplicationDirectory;
            try
            {
                await CheckForCourseNameUpdates();
                await GetUpdates();
                
            }
            catch (Exception ex)
            {
                await AppLogger.WriteLineAsync(ex.Message, AppLogger.MessageType.HandledException);
            }
        }

        /// <summary>
        /// Every 12 hours, grab the courses containing the subscribed subjects and perform update checks
        /// </summary>
        /// <returns></returns>
        // This function is run on initialization ONCE
        public async Task GetUpdates()
        {
            DateTime timeFromLastUpdate = LocalAppSettings.GetInstance().UserStats.LastUpdateTime;
            // ToList() in order to create a copy of subscriptions and to not have subs be a reference to subscriptions
            List<Subject> subs = LocalAppSettings.GetInstance().UserConfig.Subscriptions.ToList();
            List<string> scheduledCourses = new();
            bool updated = false;

            // Check if any course files are empty
            var courseInfo = CourseNameGetModel.Get().CourseInfo;
            for (int i = 0; i < courseInfo.Count; i++)
            {
                var course = JsonManager.ReadCourseFromFile(courseInfo[i].CourseName);
                if (courseInfo[i].CourseName == "ΑΝΑΚΟΙΝΩΣΕΙΣ")
                    continue;
                if (course == null)
                    scheduledCourses.Add(courseInfo[i].CourseName);
            }

            // Download courses that have empty JSON files
            List<Course> downloadedCourses = await new CourseBuilder().GetCoursesAsync(scheduledCourses);

            if (downloadedCourses.Count > 0)
                updated = true;

            // Download subscribed subjects if 12 or more hours have passed since the last update
            if (timeFromLastUpdate.AddHours(12) <= DateTime.Now)
            {
                List<Subject> updatedSubs = new();
                List<Task<Subject>> downloadTasks = new();
                CourseBuilder builder = new();
                for (int i = 0; i < subs.Count; i++)
                {
                    if (downloadedCourses.Any(course => course.Subjects.Contains(subs[i])))
                    {
                        continue;
                    }
                    // Instantiates a new Subject because GetSubjectContent modifies the Subject object that is passed in the arguments.
                    // We want to retain the old subject information for the UpdateChecker to do its job.
                    downloadTasks.Add(builder.GetSubjectContent(new Subject(subs[i])));
                }

                // Download all subjects in parallel
                await Task.WhenAll(downloadTasks);
                downloadTasks.ForEach(task => updatedSubs.Add(task.Result));

                // Perform update checks
                Stopwatch s = Stopwatch.StartNew();
                Dictionary<Subject, DocumentCollection> subUpdates = new();
                for (int i = 0; i < updatedSubs.Count; i++)
                {
                    subUpdates.Add(updatedSubs[i], new UpdateChecker().GetSubjectUpdates(subs[i], updatedSubs[i]));
                }
                s.Stop();
                await AppLogger.WriteLineAsync($"Update checking complete in {(double)s.ElapsedMilliseconds / 1000}s");

                LocalAppSettings.GetInstance().UserConfig.Subscriptions = updatedSubs;
                LocalAppSettings.GetInstance().SaveSettings();

                updated = true;
                AppEventAggregator.Publish(new UpdateCompleteEvent(subUpdates));
            }

            if (updated)
                LocalAppSettings.GetInstance().UserStats.LastUpdateTime = DateTime.Now;
        }

        /// <summary>
        /// Updates every month
        /// </summary>
        private async Task<List<string>> CheckForCourseNameUpdates()
        {
            string courseNamesPath = $"{Shared.ConfigDirectory}/course_names.json";
            var courseNamesModel = JsonManager.ReadJsonFromFile<CourseNameGetModel>(courseNamesPath);
            List<(string courseName, string courseId)> courseInfos = new List<(string, string)>();
            if (courseNamesModel == null || 
                courseNamesModel.LastCheckDate.AddMonths(1) <= DateTime.Now || 
                courseNamesModel.CourseInfo.Count == 0)
            {
                CourseBuilder builder = new();
                List<(string courseName, string, string courseId)> courses = await builder.GetCoursesInfoAsync();

                for (int i = 0; i < courses.Count; i++)
                {
                    courseInfos.Add((courses[i].courseName, courses[i].courseId));
                }

                JsonManager.WriteJsonToFile<CourseNameGetModel>(new()
                {
                    LastCheckDate = DateTime.Now,
                    CourseInfo = courseInfos
                }, courseNamesPath);
            }
            else
            {
                courseInfos = courseNamesModel.CourseInfo;
            }
            return courseInfos.Select(course => course.courseName).ToList();
        }

        private void Desktop_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            var appSettings = LocalAppSettings.GetInstance();
            appSettings.UserStats.LastRunTime = DateTime.Now;
            appSettings.SaveSettings();

            AppLogger.WriteLine($"CONTROLLED APPLICATION EXIT (CODE {e.ApplicationExitCode})");
        }
    }
}