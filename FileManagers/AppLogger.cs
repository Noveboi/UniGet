using System.Text;

namespace FileManagers
{
    public static class AppLogger
    {
        private const string _path = $"{Shared.ConfigDirectory}/log.txt";
        private static DateTime lastWrite = DateTime.MinValue;
        private readonly static TimeSpan timeForNL = TimeSpan.FromMinutes(1);
        
        public static void WriteLine(string text, MessageType msgType = MessageType.Status)
        {
            byte[] strBytes = MakeString(text, msgType);

            using var file = new FileStream(_path, FileMode.Append);
            long fileBytes = file.Length;

            file.Write(strBytes, 0, strBytes.Length);
        }

        public static async Task WriteLineAsync(string text, MessageType msgType = MessageType.Status)
        {
            byte[] strBytes = MakeString(text, msgType);

            using var file = new FileStream(_path, FileMode.Append);
            long fileBytes = file.Length;

            await file.WriteAsync(strBytes, 0, strBytes.Length);
        }

        public static void ClearLog()
        {
            using var file = new FileStream(_path, FileMode.Create);
        }

        public enum MessageType
        {
            Status,
            HandledException
        }

        private static byte[] MakeString(string text, MessageType msgType)
        {
            DateTime now = DateTime.Now;
            string output = string.Empty;

            if (now - lastWrite >= timeForNL)
                output += Environment.NewLine;

            if (msgType == MessageType.Status)
            {
                output += "(STATUS) ";
            }
            else if (msgType == MessageType.HandledException)
            {
                output += "(HAN_EX)";
            }
            output += $"[{now.ToString("dd/MM/yyyy HH:mm:ss:fff")}] - ";
            output += text;
            output += Environment.NewLine;

            lastWrite = now;
            return Encoding.UTF8.GetBytes(output);
        }
    }
}
