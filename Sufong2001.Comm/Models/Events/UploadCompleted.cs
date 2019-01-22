using System;
using System.Collections.Generic;
using System.Text;

namespace Sufong2001.Comm.AzureStorage
{
    public class UploadCompleted
    {
        public string SessionId { get; set; }

        public int TryCount { get; set; }
    }
}
