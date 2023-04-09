using University;

namespace FileManagers
{
    internal class DirectoryStack
    {

        private Stack<(string, DocumentCollection)> _dirStack;
        private Mode _mode;
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

        public void Push(string directoryName, DocumentCollection documents)
        {
            _dirStack.Push((directoryName, documents));
            string fullPath = GetCwd();
        }
        public (string, DocumentCollection) Pop() => _dirStack.Pop();
        public string GetCwd() => Shared.ApplicationDirectory + "/" + GetPath();
        public string GetPath()
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
