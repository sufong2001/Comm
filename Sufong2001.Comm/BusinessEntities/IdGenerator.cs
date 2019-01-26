using System;
using Sufong2001.Comm.Interfaces;

namespace Sufong2001.Comm.BusinessEntities
{
    public class IdGenerator : IUploadIdGenerator, IMessageIdGenerator
    {
        public string UploadSessionId()
        {
            return Guid.NewGuid().ToString();
        }

        public string MessageId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
