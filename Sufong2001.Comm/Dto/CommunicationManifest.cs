namespace Sufong2001.Comm.Dto
{
    public class CommunicationManifest
    {
        public string Reference { get; set; }

        public string Title { get; set; }

        public Email Email { get; set; }

        public Sms Sms { get; set; }

        public Postage Postage { get; set; }

        /// <summary>
        /// The list of the attachment file name for all the message which must be upload along with
        /// the manifest.
        /// </summary>
        public string[] Attachments { get; set; }
    }
}