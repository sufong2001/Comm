using System;
using System.Collections.Generic;
using System.Text;
using Sufong2001.Comm.Interfaces;

namespace Sufong2001.Comm.Models
{
    public class IdGenerator : ITransferIdGenerator
    {
        public string TransferSessionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
