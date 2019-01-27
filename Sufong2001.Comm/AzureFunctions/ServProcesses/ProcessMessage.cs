using AzureFunctions.Autofac;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Sufong2001.Comm.AzureFunctions.Names;
using Sufong2001.Comm.AzureStorage;
using Sufong2001.Comm.Configurations.Resolvers;
using Sufong2001.Comm.Dto;
using Sufong2001.Comm.Interfaces;
using Sufong2001.Share.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sufong2001.Comm.AzureStorage.Names;
using Sufong2001.Comm.Models.Events;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Sufong2001.Comm.AzureFunctions.ServProcesses
{
    [DependencyInjectionConfig(typeof(DiConfig))]
    public static class CommProcessors
    {
        [FunctionName(OrchestratorNames.ProcessMessage)]
        public static async Task<object> ProcessMessageOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContextBase ctx,
            ILogger log)
        {
            var uploadCompleted = ctx.GetInput<UploadCompleted>();

            try
            {
                var result = await ctx.CallActivityAsync<IList<TableResult>>(ActivityNames.ProcessManifest, uploadCompleted);

                log.Log(LogLevel.Information, result.ToJson());

                return result;
            }
            catch (Exception e)
            {
            }

            return new { Error = "Failed to process message" };
        }

        [FunctionName(ActivityNames.ProcessManifest)]
        public static async Task<IList<TableResult>> ProcessManifestActivity([ActivityTrigger] UploadCompleted uploadCompleted,
            [Blob(BlobNames.UploadDirectory + "/{uploadCompleted.SessionId}/" + CommunicationManifest.FileName)] CloudBlockBlob manifest,
            [Table(TableNames.CommMessage)] CloudTable messageTable,
            [Inject()] IMessageIdGenerator idGenerator,
            ILogger log)
        {
            var json = await manifest.DownloadTextAsync();

            var communicationManifest = json.To<CommunicationManifest>();

            var results = await communicationManifest.PrepareCommMessage(uploadCompleted.SessionId)
                .CreateIn(messageTable);

            log.Log(LogLevel.Information, json);

            return results;
        }
    }
}