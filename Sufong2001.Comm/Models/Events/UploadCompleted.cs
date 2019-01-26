namespace Sufong2001.Comm.Models.Events
{
    public class UploadCompleted
    {
        public string SessionId { get; set; }

        public int TryCount { get; set; }
    }
}
