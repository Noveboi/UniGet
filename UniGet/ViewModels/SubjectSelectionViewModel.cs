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
        public ObservableCollection<ListNode> Courses { get; }
        public ObservableCollection<ListNode> SelectedSubjects { get; }
        public ReactiveCommand<Unit,Unit> Subscribe { get; }
        public SubjectSelectionViewModel(ObservableCollection<SubjectNode> subjectNodes) : this()
        {
            _subjectNodes = subjectNodes;
        }

        public SubjectSelectionViewModel()
        {
            SelectedSubjects = new ObservableCollection<ListNode>();
            Subscribe = ReactiveCommand.Create(SubscribeToSelectedSubjects);
            Courses = new ObservableCollection<ListNode>();

            ReadFromCoursesAndSetupTree
                (JsonManager.
                ReadJsonFromFile<CourseNameGetModel>
                ($"{Shared.ConfigDirectory}/course_names.json")
                .CourseInfo.Select(tup => tup.CourseName).ToList());
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

        private void ReadFromCoursesAndSetupTree(List<string> courseNames)
        {
            List<Course> courses = new();
            ListNode root = new("Root");
            // i = 1, to skip the course ΑΝΑΚΟΙΝΩΣΕΙΣ
            for (int i = 1; i < courseNames.Count; i++)
            {
                courses.Add(JsonManager.ReadCourseFromFile(courseNames[i]));
                // Add the course names
                root.Children.Add(new ListNode(courseNames[i]));
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
                        (new ListNode(courses[i].Subjects[j].Name) 
                        { Subject = courses[i].Subjects[j] });
                }
                Courses.Add(root.Children[i]);
            }
        }
    }
}
