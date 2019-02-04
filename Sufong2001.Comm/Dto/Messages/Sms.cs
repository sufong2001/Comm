using Sufong2001.Comm.Dto.Interfaces;

namespace Sufong2001.Comm.Dto.Messages
{
    public class Sms: IMessage
    {
        public bool IsUrgent { get; set; }

        public string Mobile { get; set; }

        public string SmsContent { get; set; }
    }
}