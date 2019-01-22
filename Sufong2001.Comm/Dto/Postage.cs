using Sufong2001.Comm.Dto.Interfaces;
using Sufong2001.Share.Json;

namespace Sufong2001.Comm.Dto
{
    public class Postage : IAttachments
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Company { get; set; }

        public Address Address { get; set; }

        public string[] Attachments { get; set; }

        public ToAddress ToAddress() => new object[] { Address, this }.MergeTo<ToAddress>();

        public string[] PostalAddress() => ToAddress().ToLines();
    }
}