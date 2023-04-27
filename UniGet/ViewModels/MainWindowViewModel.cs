using Avalonia.Controls;
using DynamicData;
using FileManagers;
using Network;
using ReactiveUI;
using Scraper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        private SubjectNode _selectedSubject;
        private string _selectedSubName = "Select a subject.";
        private int _selectedDocIdx;
        private int _selectedUpdateDocIdx;
        private bool _backButtonVisible;
        // Do NOT confuse this with the FileManagers.DirectoryStack, this stack is used for UI purposes, not file management
        private DataGridDirectoryStack _dirStack = new DataGridDirectoryStack();
        private string _singleProgText;
        private string _multiProgText;
        public string SingleProgText
        {
            get => _singleProgText;
            set => this.RaiseAndSetIfChanged(ref _singleProgText, value);
        }
        public string MultiProgText
        {
            get => _multiProgText;
            set => this.RaiseAndSetIfChanged(ref _multiProgText, value);
        }
        /// <summary>
        /// Displays the subjects in the left-hand side of the <see cref="MainWindow"/>
        /// </summary>
        public ObservableCollection<SubjectNode> SubjectsList { get; private set; }
        /// <summary>
        /// Displays the selected subject's documents in the DataGrid
        /// </summary>
        public ObservableCollection<DocumentDataGridModel> SubjectDocs { get; } = new();
        public ObservableCollection<DocumentDataGridModel> SubjectUpdates { get; } = new();
        /// <summary>
        /// Gets and sets the subject that user last selected from the SubjectsList
        /// </summary>
        public SubjectNode SelectedSubject
        {
            get => _selectedSubject;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedSubject, value);
                SelectedSubject_Changed(_selectedSubject, new System.ComponentModel.PropertyChangedEventArgs(nameof(SelectedSubject)));
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
                    SelectedSubjectName = _dirStack.GetTitleString();

                    if (_dirStack.Count > 1)
                    {
                        BackButtonVisible = true;
                    }
                }
            }
        }

        /// <summary>
        /// Gets and sets the index of the selected document in the UPDATES DataGrid
        /// </summary>
        public int SelectedUpdateDocIdx
        {
            get => _selectedUpdateDocIdx;
            set => this.RaiseAndSetIfChanged(ref _selectedUpdateDocIdx, value);
        }
        
        public bool BackButtonVisible
        {
            get => _backButtonVisible;
            set => this.RaiseAndSetIfChanged(ref _backButtonVisible, value);
        }

        public MainWindowViewModel()
        {
            ProgressReporter.OngoingProgressAmountChanged += MultiProgressChanged;
            ProgressReporter.DownloadProgressChanged += SingleProgressChanged;
            SubjectsList = SetupSubjectsTree();
            if (SubjectsList.Count > 0)
            {
                SelectedSubject = SubjectsList[0];
            }

            AppEventAggregator.Subscribe<UpdateCompleteEvent>(HandleUpdateComplete);
        }

        /// <summary>
        /// Opens explorer.exe for windows, and the corresponding explorers for other OS
        /// </summary>
        public void OpenDocDirectory()
        {
            string path = LocalAppSettings.GetInstance().UserConfig.ApplicationDirectory;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ProcessStartInfo explorerStartInfo = new ProcessStartInfo("explorer.exe")
                {
                    Arguments = path
                };
                Process.Start(explorerStartInfo);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                ProcessStartInfo finderStartInfo = new ProcessStartInfo("open")
                {
                    Arguments = path
                };
                Process.Start(finderStartInfo);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                ProcessStartInfo explorerStartInfo = new ProcessStartInfo("nautilus")
                {
                    Arguments = path
                };
                Process.Start(explorerStartInfo);
            }
        }

        public async Task ChangeDocDirectory()
        {
            await SetNewDocDirectory();
        }

        /// <summary>
        /// When App.xaml.cs finishes update checking, receive the updated documents foreach subject and display: 
        ///   The number of updated documents for each subject and
        ///   the updated documents on a small DataGrid for quick access
        /// </summary>
        private void HandleUpdateComplete(UpdateCompleteEvent updateCompleteEvent)
        {
            var allUpdatedDocuments = updateCompleteEvent.updateDocuments;
            foreach (KeyValuePair<Subject, DocumentCollection> pair in allUpdatedDocuments)
            {
                SubjectNode updatedNode = new SubjectNode(pair.Key, pair.Value);
                SubjectsList.Replace(FindSubjectInList(pair.Key), updatedNode);
            }
        }

        /// <summary>
        /// Called when user clicks the "Download" button in the DataGrid
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task DownloadDocAtIndex(object param)
        {
            // Check if current directory exists
            if (!Directory.Exists(Shared.ApplicationDirectory))
            {
                await SetNewDocDirectory();
            }

            // If a folder is given for download, and its contents and not the folder itself.
            DocumentDataGridModel doc = (DocumentDataGridModel)param;
            DocumentCollection docsToDownload = new();

            if (doc.File.Type == DocType.Dir)
            {
                docsToDownload.Files.Add((doc.File as Folder).ExtractFiles());
            }
            else
            {
                docsToDownload.Files.Add(doc.File as Document);
            }

            await new FileManager().DownloadSubjectAsync(SelectedSubject.Subject, docsToDownload);
        }

        public async Task DownloadUpdateAtIndex(object param)
        {
            // Check if current directory exists
            if (!Directory.Exists(Shared.ApplicationDirectory))
            {
                await SetNewDocDirectory();
            }

            DocumentDataGridModel doc = (DocumentDataGridModel)param;
            DocumentCollection docsToDownload = new();
            docsToDownload.Files.Add(doc.File as Document);

            await new FileManager().DownloadSubjectAsync(SelectedSubject.Subject, docsToDownload);
            SubjectUpdates.Remove(doc);
        }

        private async Task SetNewDocDirectory()
        {
            OpenFolderDialog dialog = new OpenFolderDialog();
            dialog.Title = "Select a folder to store your downloaded documents";
            string? newPath = await dialog.ShowAsync(MainWindow.Instance);

            if (newPath != null)
            {
                string oldPath = LocalAppSettings.GetInstance().UserConfig.ApplicationDirectory;
                Shared.ApplicationDirectory = newPath;
                LocalAppSettings.GetInstance().SetNewAppDirectory(newPath);
                LocalAppSettings.GetInstance().SaveSettings();

                await AppLogger.WriteLineAsync($"User changed document directory from {oldPath} to {newPath}");
            }
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
            SubjectUpdates.Clear();
            _dirStack.PopUntilSubject();
            BackButtonVisible = false;

            if (sender == null)
            { 
                return; 
            }

            // Problem: in App.axaml.cs, the downloading, update checking, etc... is async. The problem lies in the fact that SubjectNodess
            // is initialized once BEFORE update checking is done and therefore any new documents are NOT displayed on the screen

            SubjectNode subjectNode = (SubjectNode)sender;

            if (subjectNode.Documents != null)
            {
                for (int i = 0; i < subjectNode.Documents.Count; i++)
                {
                    SubjectDocs.Add(subjectNode.Documents[i]);
                }
                _dirStack.Push(subjectNode);
            }

            if (subjectNode.HasUpdates)
            {
                for (int i = 0; i < subjectNode.Updates.Count; i++)
                {
                    SubjectUpdates.Add(subjectNode.Updates[i]);
                }
            }

            SelectedSubjectName = _dirStack.GetTitleString();
        }

        public void SingleProgressChanged(object? sender, SingleProgressInfoEventArgs e)
        {
            if (e.DownloadComplete)
            {
                SingleProgText = $"Finished downloading {e.Downloadee}";
            }
            else
            {
                SingleProgText = $"Downloading {e.Downloadee} ({e.Percentage:f2}%)";
            }
        }
        public void MultiProgressChanged(object? sender, MultiProgressInfoEventArgs e)
        {
            if (e.OngoingProgCount == 0)
                MultiProgText = string.Empty;
            else
                MultiProgText = $"{e.OngoingProgCount} remaining...";
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

        private SubjectNode? FindSubjectInList(Subject subject)
        {
            for (int i = 0; i < SubjectsList.Count; i++)
            {
                if (SubjectsList[i].Subject.Equals(subject))
                    return SubjectsList[i];
            }

            return null;
        }

        public void GoToPreviousDirectory()
        {
            if (_dirStack.Count <= 1)
            {
                BackButtonVisible = false;
                return;
            }
            _dirStack.Pop();
            SelectedSubjectName = _dirStack.GetTitleString();

            SubjectDocs.Clear();
            SubjectDocs.AddRange(_dirStack.Peek());
        }
    }
}