using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniGet.Models;
using Scraper;
using System.Diagnostics;
using ReactiveUI;
using System.Reactive;
using FileManagers;
using University;
using UniGet.Models.AppSettings;
using System.IO;

namespace UniGet.ViewModels
{
    public class SubjectSelectionViewModel : ViewModelBase
    {
        private ObservableCollection<SubjectNode> _subjectNodes;
        public ObservableCollection<TreeNode> Courses { get; }
        public ObservableCollection<TreeNode> SelectedSubjects { get; }
        public ReactiveCommand<Unit,Unit> Subscribe { get; }
        public SubjectSelectionViewModel(ObservableCollection<SubjectNode> subjectNodes) : this()
        {
            _subjectNodes = subjectNodes;
        }

        public SubjectSelectionViewModel()
        {
            SelectedSubjects = new ObservableCollection<TreeNode>();
            Subscribe = ReactiveCommand.Create(SubscribeToSelectedSubjects);
            Courses = new ObservableCollection<TreeNode>();
            new Action(async () =>
            {
                ReadFromCoursesAndSetupTree(await UpdateCourses());
            })();
        }

        private void SubscribeToSelectedSubjects()
        {
            for (int i = 0; i < SelectedSubjects.Count; i++)
            {
                var subject = SelectedSubjects[i].Subject;
                LocalAppSettings.GetInstance().AddSubscription(subject);
                _subjectNodes.Add(new SubjectNode(subject));
            }
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
            if (courseNamesModel.LastCheckDate.AddMonths(1) <= DateTime.Now || courseNamesModel.CourseInfo.Count == 0)
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

        private void ReadFromCoursesAndSetupTree(List<string> courseNames)
        {
            List<Course> courses = new();
            TreeNode root = new("Root");
            // i = 1, to skip the course ΑΝΑΚΟΙΝΩΣΕΙΣ
            for (int i = 1; i < courseNames.Count; i++)
            {
                courses.Add(JsonManager.ReadCourseFromFile(courseNames[i]));
                // Add the course names
                root.Children.Add(new TreeNode(courseNames[i]));
            }

            // Under each course name, add its subjects
            for (int i = 0; i < courses.Count; i++)
            {
                if (courses[i] == null) 
                    continue;
                for (int j = 0; j < courses[i].Subjects.Count; j++)
                {
                    root.Children[i]
                        .Children
                        .Add
                        (new TreeNode(courses[i].Subjects[j].Name) 
                        { Subject = courses[i].Subjects[j] });
                }
                Courses.Add(root.Children[i]);
            }
        }
    }
}
