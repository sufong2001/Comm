using AzureFunctions.Autofac;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Sufong2001.Comm.AzureFunctions.Names;
using Sufong2001.Comm.Configurations.Resolvers;
using Sufong2001.Comm.Interfaces;
using System;
using System.Threading.Tasks;

namespace Sufong2001.Comm.AzureFunctions.ServProcesses
{
    [DependencyInjectionConfig(typeof(DiConfig))]
    public static class CommProcessors
    {
        [FunctionName(OrchestratorNames.ProcessMessage)]
        public static async Task<object> ProcessMessage(
            [OrchestrationTrigger] DurableOrchestrationContext ctx,
            ILogger log)
        {
            var manifestKey = ctx.GetInput<string>();

            try
            {
                var id = await ctx.CallActivityAsync<string>(ActivityNames.ProcessManifest, manifestKey);

                log.Log(LogLevel.Information, id);

                return id;
            }
            catch (Exception e)
            {
            }

            return new { Error = "Failed to process message" };
        }

        [FunctionName(ActivityNames.ProcessManifest)]
        public static async Task<string> ProcessManifest([ActivityTrigger] string manifestKey,
            [Blob("comm/upload")] CloudBlobDirectory uploadDir,
            [Table("CommUpload")] CloudTable uploadTmpTable,
            [Inject()] ITransferIdGenerator idGenerator,
            ILogger log)
        {
            var txt = await uploadDir.GetDirectoryReference(manifestKey).GetBlockBlobReference("manifest.json")
                .DownloadTextAsync();

            log.Log(LogLevel.Information, txt);

            return txt;
        }
    }
}