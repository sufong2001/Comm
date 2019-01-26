using System;
using System.Collections.Generic;
using System.Text;

namespace Sufong2001.Comm.AzureFunctions.Names
{
    public static class ServiceNames
    {
        #region communication upload

        public const string UploadStart    = "UploadStart";
        public const string UploadContinue = "UploadContinue";
        public const string UploadEnd      = "UploadEnd";

        #endregion

        #region communication processes

        public const string ProcessStarter   = "ProcessStarter";

        #endregion

        #region communication delivery

        public const string Delivery         = "Delivery";

        #endregion
    }
}
