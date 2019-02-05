using System;
using System.Linq;
using Sufong2001.Comm.Interfaces;
using Sufong2001.Share.Date;
using Sufong2001.Share.String;

namespace Sufong2001.Comm.BusinessEntities
{
    public class IdGenerator : IUploadIdGenerator, IMessageIdGenerator, IScheduleIdGenerator
    {
        public string UploadSessionId()
        {
            return Guid.NewGuid().ToString();
        }

        public string MessageId(string sessionId)
        {
            var prefix = sessionId.GuidPrefix();
            return prefix + "-" + Guid.NewGuid();
        }

        public string ScheduleId(DateTime dateTime, string separator)
        {
            return dateTime.DateGuid(separator);
        }
    }
}
