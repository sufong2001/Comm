using Sufong2001.Comm.Dto;

namespace Sufong2001.Comm.Models.Storage
{
    public class Message
    {
        public string Reference { get; set; }

        public string Title { get; set; }

        public string EmailSubject { get; set; }

        public string EmailAddress { get; set; }

        public string EmailContent { get; set; }

        public string Mobile { get; set; }

        public string SmsContent { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Company { get; set; }

        public Address Address { get; set; }

        public string AttachmentList { get; set; }
    }
}