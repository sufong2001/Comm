using AzureFunctions.Autofac;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sufong2001.Comm.AzureFunctions.Names;
using Sufong2001.Comm.AzureStorage;
using Sufong2001.Comm.AzureStorage.Names;
using Sufong2001.Comm.BusinessEntities;
using Sufong2001.Comm.Configurations.Resolvers;
using Sufong2001.Comm.Dto;
using Sufong2001.Comm.Interfaces;
using Sufong2001.Comm.Models.Events;
using Sufong2001.Comm.Models.Storage;
using Sufong2001.Comm.Models.Storage.Partitions;
using Sufong2001.Share.AzureStorage;
using Sufong2001.Share.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Blob;

namespace Sufong2001.Comm.AzureFunctions.ServProcesses
{
    [DependencyInjectionConfig(typeof(CommConfig))]
    public static class CommProcessors
    {
        [FunctionName(OrchestratorNames.ProcessMessage)]
        public static async Task<IEnumerable<MessageSchedule>> ProcessMessageOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            ILogger log)
        {
            var uploadCompleted = ctx.GetInput<UploadCompleted>();

            var messages = await ctx.CallActivityAsync<IEnumerable<Message>>(ActivityNames.ProcessManifest, uploadCompleted);

            var schedules = await ctx.CallActivityAsync<IEnumerable<MessageSchedule>>(ActivityNames.CreateSchedule, messages);

            log.Log(LogLevel.Information, schedules.ToJson());

            return schedules;
        }

        [FunctionName(ActivityNames.ProcessManifest)]
        public static async Task<IEnumerable<Message>> ProcessManifestActivity(
            [ActivityTrigger] UploadCompleted uploadCompleted,
            [Blob(BlobNames.UploadDirectory + "/{uploadCompleted.SessionId}/" + CommunicationManifest.FileName)] CloudBlockBlob manifest,
            [Table(TableNames.CommMessage)] CloudTable messageTable,
            [Inject] IMessageIdGenerator idGenerator,
            [Inject] App app,
            ILogger log)
        {
            var communicationManifest = await manifest.DownloadTextAsAsync<CommunicationManifest>();

            var results = await communicationManifest.PrepareCommMessage(uploadCompleted.SessionId, idGenerator, app.DateTimeNow)
                .CreateIn(messageTable);

            log.Log(LogLevel.Information, communicationManifest.ToJson(Formatting.Indented));

            return results.GetResult<Message>();
        }

        [FunctionName(ActivityNames.CreateSchedule)]
        public static async Task<IEnumerable<MessageSchedule>> CreateScheduleActivity(
            [ActivityTrigger] IEnumerable<Message> messages,
            [Table(TableNames.CommSchedule)] CloudTable scheduleTable,
            [Inject] IScheduleIdGenerator idGenerator,
            [Inject] App app,
            ILogger log)
        {
            var result = await messages.Where(m => m.DeliveryOrder == 0)
                .Select(m => m.IsOrMap<MessageSchedule>())
                .CreateIn(scheduleTable,
                    m => CommSchedulePartitionKeys.Scheduled,
                    m => idGenerator.ScheduleId(m.DeliverySchedule)
                );

            return result.GetResult<MessageSchedule>();
        }
    }
}