using System;

namespace Sufong2001.Comm.Models.Storage
{
    public class MessageSchedule
    {
        public string SessionId { get; set; }

        public string CommunicationReference { get; set; }

        public string RecipientReference { get; set; }

        /// <summary>
        /// The unique key of the message
        /// </summary>
        public string MessageReference { get; set; }

        public string Type { get; set; }

        public DateTime DeliverySchedule { get; set; }

        public DateTime? Delivered { get; set; }
    }
}