using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
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
            try
            {
                await UpdateCourses();
                await GetUpdates();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
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
        // TODO - UPDATE CHECKS
        // TODO - UPDATE CHECKS
        // TODO - UPDATE CHECKS
        // TODO - UPDATE CHECKS
        // TODO - UPDATE CHECKS
        // TODO - UPDATE CHECKS
        // TODO - UPDATE CHECKS
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

                LocalAppSettings.GetInstance().UserStats.LastUpdateTime = DateTime.Now;
            }

            await new CourseBuilder().GetCoursesAsync(scheduledCourses);

        }
        /// <summary>
        /// Updates every month
        /// </summary>
        /// <returns></returns>
        private async Task<List<string>> UpdateCourses()
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
                    Debug.WriteLine(courseInfos[i]);
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