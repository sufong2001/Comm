using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Sufong2001.Comm.AzureFunctions.Names;
using Sufong2001.Comm.Models.Events;
using System.Threading.Tasks;

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