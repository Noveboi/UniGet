﻿using FileManagers;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UniGet.Models
{
    /// <summary>
    /// Remembers the directory structure when the user traverses folders/directories in the DataGrid view
    /// </summary>
    internal class DataGridDirectoryStack
    {
        private readonly Stack<ObservableCollection<DocumentDataGridModel>> _dirStack = new();
        private readonly Stack<string> _dirNames = new();

        public int Count => _dirStack.Count;

        /// <summary>
        /// Push a <see cref="DocumentDataGridModel"/> into the stack
        /// </summary>
        /// <param name="folder"></param>
        public void Push(DocumentDataGridModel folder)
        {
            if (folder.Docs == null)
            {
                AppLogger.WriteLine("null Docs given.");
                return;
            }

            _dirStack.Push(folder.Docs);
            _dirNames.Push(folder.Name);
        }

        /// <summary>
        /// Push a <see cref="SubjectNode"/> subject into the stack
        /// </summary>
        /// <param name="subject"></param>
        public void Push(SubjectNode subject)
        {
            _dirStack.Push(subject.Documents);
            _dirNames.Push(subject.Subject.Name);
        }

        public void Pop()
        {
            if (_dirStack.Count > 0)
            {
                _dirStack.Pop();
                _dirNames.Pop();
            }
        }

        public void PopUntilSubject()
        {
            while (_dirStack.Count >= 1)
            {
                _dirStack.Pop();
                _dirNames.Pop();
            }
        }

        public ObservableCollection<DocumentDataGridModel> Peek()
        {
            return _dirStack.Peek();
        }

        public string GetTitleString()
        {
            string title = string.Empty;
            string[] dirNamesCopy = new string[_dirNames.Count];
            _dirNames.CopyTo(dirNamesCopy, 0);
            for (int i = dirNamesCopy.Length - 1; i >= 0; i--)
            {
                if (i == dirNamesCopy.Length - 1)
                {
                    title += dirNamesCopy[i];
                    continue;
                }

                title += $" / {dirNamesCopy[i]}";
            }

            return title;
        }
    }
}
