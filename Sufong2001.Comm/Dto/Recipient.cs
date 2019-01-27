using Sufong2001.Comm.Dto.Messages;

namespace Sufong2001.Comm.Dto
{
    public class Recipient
    {
        public string RecipientReference { get; set; }

        public Email Email { get; set; }

        public Sms Sms { get; set; }

        public Postage Postage { get; set; }

        /// <summary>
        /// The list of the attachment file name for the message pack only
        /// </summary>
        public string[] Attachments { get; set; }
    }
}