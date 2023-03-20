using Avalonia.Controls;
using DynamicData;
using FileManagers;
using ReactiveUI;
using Scraper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using UniGet.Models;
using UniGet.Models.AppSettings;
using UniGet.Views;
using University;

namespace UniGet.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private TreeViewItemViewModel _selectedSubject = new TreeViewItemViewModel();
        private string _selectedSubName = "Select a subject.";
        private int _selectedDocIdx;
        private bool _backButtonVisible;
        // Do NOT confuse this with the FileManagers.DirectoryStack, this stack is used for UI purposes, not file management
        private DataGridDirectoryStack _dirStack = new DataGridDirectoryStack();
        /// <summary>
        /// Displays the subjects in the left-hand side of the <see cref="MainWindow"/>
        /// </summary>
        public ObservableCollection<SubjectNode> SubjectsList { get; private set; }
        /// <summary>
        /// Displays the selected subject's documents in the DataGrid
        /// </summary>
        public ObservableCollection<DocumentDataGridModel> SubjectDocs { get; } = new();
        /// <summary>
        /// Gets and sets the subject that user last selected from the SubjectsList
        /// </summary>
        public SubjectNode SelectedSubject
        {
            get => _selectedSubject.Node;
            set
            {
                _selectedSubject.Node = value;
                _selectedSubject.OnPropertyChanged();
            }
        }
        public string SelectedSubjectName
        {
            get => _selectedSubName;
            set => this.RaiseAndSetIfChanged(ref _selectedSubName, value);
        }
        /// <summary>
        /// Gets and sets the index of the selected document in the DataGrid 
        /// </summary>
        public int SelectedDocIdx
        {
            get => _selectedDocIdx;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedDocIdx, value);
                // If the new selected document happens to be a folder, traverse into its contents
                if (SubjectDocs.Count == 0 || value < 0) 
                    return;
                DocumentDataGridModel newSelectedDoc = SubjectDocs[value];
                if (newSelectedDoc.File.Type == DocType.Dir)
                {
                    SubjectDocs.Clear();
                    SubjectDocs.AddRange(newSelectedDoc.Docs);
                    _dirStack.Push(newSelectedDoc);
                    SelectedSubjectName = _dirStack.GetTitleName();

                    if (_dirStack.Count > 1)
                    {
                        BackButtonVisible = true;
                    }
                }
            }
        }
        
        public bool BackButtonVisible
        {
            get => _backButtonVisible;
            set => this.RaiseAndSetIfChanged(ref _backButtonVisible, value);
        }

        public MainWindowViewModel()
        {
            _selectedSubject.PropertyChanged += SelectedSubject_Changed;
            SubjectsList = SetupSubjectsTree();

            Debug.WriteLine("INIT!");
        }

        public async Task DownloadDocAtIndex(object param)
        {
            // SEPARATE BEHAVIOR FOR FILE AND FOLDER
            DocumentDataGridModel doc = (DocumentDataGridModel)param;
            DocumentCollection docsToDownload = new();
            Debug.WriteLine(doc.Name);
            Debug.WriteLine(doc.File.DownloadLink);

            if (doc.File.Type == DocType.Dir)
            {
                docsToDownload.Folders.Add(doc.File as Folder);
            }
            else
            {
                docsToDownload.Files.Add(doc.File as Document);
            }

            FileDownloader fd = new FileDownloader();
            await fd.DownloadSubjectAsync(SelectedSubject.Subject, docsToDownload);
        }

        public void RemoveSubject()
        {
            Subject subjectToRemove = LocalAppSettings.GetInstance()
                .UserConfig
                .Subscriptions
                .Find
                (subject => subject.Name == SelectedSubject.Subject.Name) ?? new Subject();
            SubjectsList.Clear();
            var newSubs = LocalAppSettings.GetInstance().RemoveSubscription(subjectToRemove);
            newSubs.ForEach(sub => SubjectsList.Add(new SubjectNode(sub)));
        }

        public void OpenSubSelectWindow()
        {
            new SubjectSelection(SubjectsList).Show();
        }

        private void SelectedSubject_Changed(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SubjectDocs.Clear();
            _dirStack.PopUntilSubject();
            BackButtonVisible = false;

            if (sender == null) return;
            SubjectNode subjectNode = ((TreeViewItemViewModel)sender).Node;

            if (subjectNode.Documents != null)
            {
                for (int i = 0; i < subjectNode.Documents.Count; i++)
                {
                    SubjectDocs.Add(subjectNode.Documents[i]);
                }
                _dirStack.Push(subjectNode);
            }

            SelectedSubjectName = _dirStack.GetTitleName();
        }

        /// <summary>
        /// Adds the subject names to the TreeView, subjects are taken from the subscriptions
        /// </summary>
        /// <returns></returns>
        private ObservableCollection<SubjectNode> SetupSubjectsTree()
        {
            LocalAppSettings appSettings = LocalAppSettings.GetInstance();
            Subject[] subjects = new Subject[appSettings.UserConfig.Subscriptions.Count];
            
            for (int i = 0; i < subjects.Length; i++)
            {
                subjects[i] = appSettings.UserConfig.Subscriptions[i]; 
            }

            var root = new ObservableCollection<SubjectNode>();
            foreach(var subject in subjects)
            {
                root.Add(new SubjectNode(subject));
            }

            return root;
        }

        public void GoToPreviousDirectory()
        {
            if (_dirStack.Count <= 1)
            {
                BackButtonVisible = false;
                return;
            }
            _dirStack.Pop();
            SelectedSubjectName = _dirStack.GetTitleName();

            SubjectDocs.Clear();
            SubjectDocs.AddRange(_dirStack.Peek());
        }
    }
}