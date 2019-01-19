using Sufong2001.Share.String;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sufong2001.Comm.Models
{
    public class CommunicationManifest
    {
        public IList<MessageMetaData> Messages { get; set; }
        public IList<string> Attachments { get; set; }
    }

    public class MessageMetaData
    {
        public string Reference { get; set; }

        public string Subject { get; set; }

        public string SmsContent { get; set; }

        public string EmailContent { get; set; }

        public string CoverLetter { get; set; }

        public Contact Recipient { get; set; }

        public IList<string> Attachments { get; set; }
    }

    public class Contact
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Company { get; set; }

        public string Email { get; set; }

        public string Mobile { get; set; }

        public Address Address { get; set; }
    }

    public class Address
    {
        public string Line1 { get; set; }

        public string Line2 { get; set; }

        public string Line3 { get; set; }

        public string State { get; set; }

        public string Postcode { get; set; }

        public string Country { get; set; }

        public string[] ToLines()
        {
            return new[]
            {
                Line1, Line2, Line3, $"{State} {Postcode}", Country
            }.Where(l => string.IsNullOrEmpty(l)).ToArray();
        }

        public override string ToString()
        {
            return ToLines().ArrayToString(Environment.NewLine);
        }
    }
}