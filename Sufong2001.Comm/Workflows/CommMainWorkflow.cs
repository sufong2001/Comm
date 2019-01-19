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

            run = ServiceNames.TransferStart;

            run = ServiceNames.TransferContinue;

            run = ServiceNames.TransferEnd;

            run = ServiceNames.ProcessStarter;

            run = ServiceNames.Delivery;

        }
    }
}
