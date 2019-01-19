using System;
using System.Collections.Generic;
using System.Text;

namespace Sufong2001.Comm.AzureFunctions.Names
{
    static class ServiceNames
    {
        #region communication upload

        public const string TransferStart    = "TransferStart";
        public const string TransferContinue = "TransferContinue";
        public const string TransferEnd      = "TransferEnd";

        #endregion

        #region communication processes

        public const string ProcessStarter   = "ProcessStarter";

        #endregion

        #region communication delivery

        public const string Delivery         = "Delivery";

        #endregion
    }
}
