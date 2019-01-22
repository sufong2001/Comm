using AzureFunctions.Autofac;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Sufong2001.Comm.AzureFunctions.Names;
using Sufong2001.Comm.AzureStorage;
using Sufong2001.Comm.Configurations.Resolvers;
using Sufong2001.Comm.Interfaces;
using Sufong2001.Comm.Models;
using Sufong2001.Share.Arrays;
using Sufong2001.Share.Json;
using Sufong2001.Share.String;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sufong2001.Comm.Dto;
using Sufong2001.Comm.Models.Storage;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

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
            var uploadCompleted = ctx.GetInput<UploadCompleted>();

            try
            {
                var manifest = await ctx.CallActivityAsync<CommunicationManifest>(ActivityNames.ProcessManifest, uploadCompleted);

                log.Log(LogLevel.Information, manifest.ToJson());

                return manifest;
            }
            catch (Exception e)
            {
            }

            return new { Error = "Failed to process message" };
        }

        [FunctionName(ActivityNames.ProcessManifest)]
        public static async Task<CommunicationManifest> ProcessManifest([ActivityTrigger] UploadCompleted uploadCompleted,
            [Blob("comm/upload")] CloudBlobDirectory uploadDir,
            [Table("CommMessage")] CloudTable messageTable,
            [Inject()] IMessageIdGenerator idGenerator,
            ILogger log)
        {
            await messageTable.CreateIfNotExistsAsync();

            var manifestDir = uploadDir.GetDirectoryReference(uploadCompleted.SessionId);

            var json = await manifestDir.GetBlockBlobReference("manifest.json")
                .DownloadTextAsync();

            var manifest = json.To<CommunicationManifest>();

            var results = await manifest.PrepareCommMessage(uploadCompleted.SessionId)
                .CreateIn(messageTable);

            log.Log(LogLevel.Information, json);

            return manifest;
        }

        public static IEnumerable<ITableEntity> PrepareCommMessage(this CommunicationManifest cm, string manifestKey)
        {
            var entities = new object[] { cm.Sms, cm.Email, cm.Postage }.Select((m, i) =>
            {
                var e = m.JClone<Message>();

                e.Reference = cm.Reference;
                e.Title = cm.Title;

                if (m is Dto.Interfaces.IAttachments att)
                {
                    e.AttachmentList = new[] { cm.Attachments, att.Attachments }
                        .Merge().ArrayToString(Environment.NewLine);
                }

                return new TableEntityAdapter<Message>(e, manifestKey, $"{manifestKey}-{i}");
            });

            return entities;
        }
    }
}