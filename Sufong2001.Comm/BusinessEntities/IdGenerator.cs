using System;
using Sufong2001.Comm.Interfaces;

namespace Sufong2001.Comm.BusinessEntities
{
    public class IdGenerator : ITransferIdGenerator, IMessageIdGenerator
    {
        public string TransferSessionId()
        {
            return Guid.NewGuid().ToString();
        }

        public string MessageId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
