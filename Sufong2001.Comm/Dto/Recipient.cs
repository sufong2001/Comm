using Sufong2001.Comm.Dto.Messages;
using System;
using System.Collections.Generic;

namespace Sufong2001.Comm.Dto
{
    public class Recipient
    {
        public string RecipientReference { get; set; }

        public Email Email { get; set; }

        public Sms Sms { get; set; }

        public Postage Postage { get; set; }

        public DateTime? ScheduleTime { get; set; }

        /// <summary>
        /// Recipient specific failover options
        /// </summary>
        public IEnumerable<string> FailoverOptions { get; set; }

        /// <summary>
        /// The list of the attachment file name for the message pack only
        /// </summary>
        public string[] Attachments { get; set; }
    }
}