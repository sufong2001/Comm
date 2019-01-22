using Sufong2001.Comm.Dto.Interfaces;

namespace Sufong2001.Comm.Dto
{
    public class Email : IAttachments
    {
        public string EmailSubject { get; set; }

        public string EmailAddress { get; set; }

        public string EmailContent { get; set; }

        /// <summary>
        /// The list of the attachment file name for recipient which must be upload along with
        /// the manifest.
        /// </summary>
        public string[] Attachments { get; set; }
    }
}