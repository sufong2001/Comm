using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Sufong2001.Comm.AzureFunctions.Names;
using Sufong2001.Comm.AzureStorage;
using Sufong2001.Share.Json;


namespace Sufong2001.Comm.AzureFunctions.ServProcesses
{
    public static class ProcessFunctions
    {
        [FunctionName(ServiceNames.ProcessStarter)]
        public static async Task ProcessStarter(
            [QueueTrigger("comm-process")] UploadCompleted uploadCompleted,
            [Table("CommUpload")] CloudTable uploadTable,
            [OrchestrationClient] DurableOrchestrationClient starter,
            ILogger log)
        {

            var orchestrationId = await starter.StartNewAsync(OrchestratorNames.ProcessMessage, uploadCompleted);

            log.Log(LogLevel.Information, $"Started an orchestration {orchestrationId} for uploaded manifest {uploadCompleted.SessionId}");
        }
    }
}