namespace Network
{
    public class MultiDlProgressInfo
    {
        public int DownloadsScheduled { get; set; }
        public int CompletedDownloads { get; set; }
        public float PercentComplete
        {
            get
            {
                return (float)CompletedDownloads * 100 / DownloadsScheduled;
            }
        }
    }
}
