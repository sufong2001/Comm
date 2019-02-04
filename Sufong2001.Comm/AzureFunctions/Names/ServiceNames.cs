using System;
using System.Collections.Generic;
using System.Text;

namespace Sufong2001.Comm.AzureFunctions.Names
{
    public static class ServiceNames
    {
        #region communication upload

        public const string UploadStart      = "UploadStart";
        public const string UploadContinue   = "UploadContinue";
        public const string UploadEnd        = "UploadEnd";

        #endregion

        #region communication processes

        public const string ProcessStarter   = "ProcessStarter";

        public const string Scheduler = "Scheduler";

        public const string Dispatcher = "Dispatcher";
        #endregion

        #region communication delivery

        public const string DeliverySms     = "DeliverySms";
        public const string DeliveryEmail   = "DeliveryEmail";
        public const string DeliveryPostage = "DeliveryPostage";

        #endregion
    }
}
