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
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                DoBeforeAppInit();

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
                desktop.Exit += Desktop_Exit;
            }
            base.OnFrameworkInitializationCompleted();
        }

        private async void DoBeforeAppInit()
        {

            if (!Directory.Exists(Shared.ConfigDirectory))
                Directory.CreateDirectory(Shared.ConfigDirectory);

            AppLogger.ClearLog();

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

        private void Desktop_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            var appSettings = LocalAppSettings.GetInstance();
            appSettings.UserStats.LastRunTime = DateTime.Now;
            appSettings.SaveSettings();
        }

        /// <summary>
        /// Every 12 hours, grab the courses containing the subscribed subjects and perform update checks
        /// </summary>
        /// <returns></returns>
        // This function is run on initialization ONCE
        public async Task GetUpdates()
        {
            DateTime timeFromLastUpdate = LocalAppSettings.GetInstance().UserStats.LastUpdateTime;
            List<Subject> subs = LocalAppSettings.GetInstance().UserConfig.Subscriptions;
            List<string> scheduledCourses = new();

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

            // Check if 12 hours have passed
            if (timeFromLastUpdate.AddHours(12) <= DateTime.Now)
            {
                foreach (var sub in subs)
                {
                    var subjectCourseId = sub.ID.Substring(0, 3);
                    var courseName = courseInfo.Find(course => course.CourseID.Equals(subjectCourseId)).CourseName;
                    var course = JsonManager.ReadCourseFromFile(courseName);
                    if (!scheduledCourses.Exists(c => c.Equals(course.Name)))
                        scheduledCourses.Add(course.Name);
                }
            }

            // Do not update check if no courses were downloaded
            if (scheduledCourses.Count == 0)
                return;

            // Read 'old' courses from JSON and plop them in a List
            List<Course> oldCourses = new();
            for (int i = 0; i < scheduledCourses.Count; i++)
            {
                Course course = JsonManager.ReadCourseFromFile(scheduledCourses[i]);
                oldCourses.Add(course);
            }
            List<Course> updatedCourses = await new CourseBuilder().GetCoursesAsync(scheduledCourses);

            //// Perform update checks
            //UpdateChecker updateChecker = new();
            //Stopwatch s = Stopwatch.StartNew();
            //for (int i = 0; i < oldCourses.Count; i++)
            //{
            //    for (int j = 0; j < updatedCourses[i].Subjects.Count; j++)
            //    {
            //        DocumentCollection newDocs = 
            //            updateChecker
            //            .GetSubjectUpdates(oldCourses[i].Subjects[j], updatedCourses[i].Subjects[j]);
            //    }
            //}
            //s.Stop();
            //await AppLogger.WriteLineAsync($"Update checking complete in {(double)s.ElapsedMilliseconds / 1000}s");

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
    }
}