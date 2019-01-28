using System;

namespace Sufong2001.Comm.Models.Storage
{
    public class UploadSession
    {
        //public string PartitionKey { get; set; }

        //public string RowKey { get; set; }

        public string SessionId { get; set; }

        public DateTime? UploadStart { get; set; }

        public DateTime? UploadEnd { get; set; }

        public string ManifestFile { get; set; }

        public string LastUploadedFile { get; set; }

        public string Errors { get; set; }
    }
}