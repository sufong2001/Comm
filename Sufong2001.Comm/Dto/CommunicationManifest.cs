using System;
using System.Collections.Generic;

namespace Sufong2001.Comm.Dto
{
    public class CommunicationManifest
    {
        public const string FileName = "manifest.json";

        public string CommunicationReference { get; set; }

        public string Title { get; set; }

        public IEnumerable<Recipient> Recipients { get; set; }

        public DateTime? ScheduleTime { get; set; }

        public IEnumerable<string> FailoverOptions { get; set; }

        /// <summary>
        /// The list of the attachment file name for all the message which must be upload along with
        /// the manifest.
        /// </summary>
        public string[] Attachments { get; set; }
    }
}