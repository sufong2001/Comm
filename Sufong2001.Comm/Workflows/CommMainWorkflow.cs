using Sufong2001.Comm.AzureFunctions.Names;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sufong2001.Comm.Workflows
{
    public class CommMainWorkflow
    {
        public void Run()
        {
            var run = string.Empty;

            run = ServiceNames.UploadStart;

            run = ServiceNames.UploadContinue;

            run = ServiceNames.UploadEnd;

            run = ServiceNames.ProcessStarter;

            run = ServiceNames.Scheduler;

            run = ServiceNames.Dispatcher;

            run = ServiceNames.DeliverySms;

            run = ServiceNames.DeliveryEmail;

            run = ServiceNames.DeliveryPostage;


        }
    }
}
