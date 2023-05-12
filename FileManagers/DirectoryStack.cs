using University;

namespace FileManagers
{
    /// <summary>
    /// Auxillary class that keeps track of which folders/directories we have traversed into.
    /// </summary>
    internal class DirectoryStack
    {
        private readonly Stack<(string, DocumentCollection)> _dirStack;
        private readonly Mode _mode;
        public enum Mode
        {
            /// <summary>
            /// Signifies that the <see cref="DirectoryStack"/> should create new directories 
            /// given the opportunity
            /// </summary>
            Write,
            /// <summary>
            /// Signifies that the <see cref="DirectoryStack"/> should simply traverse a directory
            /// structure and not modify anything
            /// </summary>
            Read
        }

        /// <summary>
        /// Used for the application to remember which folders (directories) it has entered
        /// </summary>
        public DirectoryStack(Mode mode)
        {
            _dirStack = new Stack<(string, DocumentCollection)>();
            _mode = mode;
        }

        /// <summary>
        /// Push into the stack the <paramref name="documents"/> of the directory with name <paramref name="directoryName"/>
        /// </summary>
        public void Push(string directoryName, DocumentCollection documents)
            => _dirStack.Push((directoryName, documents));
        /// <summary>
        /// Pop from the stack and return the popped item
        /// </summary>
        /// <returns> A 2-tuple containing the directoryName and the collection of documents that the directory contains </returns>
        public (string, DocumentCollection) Pop() => _dirStack.Pop();

        /// <summary>
        /// Get the current working directory by adding all the directories from this <see cref="DirectoryStack"/>
        /// </summary>
        public string GetCwd() => Shared.FilesDirectory + "/" + GetPath();

        /// <summary>
        /// Construct a path string from all the entries in this <see cref="DirectoryStack"/>
        /// </summary>
        /// <returns></returns>
        private string GetPath()
        {
            string path = string.Empty;
            foreach(string dirName in _dirStack.Select(tuple => tuple.Item1).Reverse())
            {
                path += dirName + "/";
            }
            return path.Remove(path.Length - 1);
        }
    }
}
