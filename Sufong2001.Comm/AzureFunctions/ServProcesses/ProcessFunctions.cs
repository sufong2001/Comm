using AzureFunctions.Autofac;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Sufong2001.Comm.AzureFunctions.Names;
using Sufong2001.Comm.AzureStorage.Names;
using Sufong2001.Comm.BusinessEntities;
using Sufong2001.Comm.Configurations.Resolvers;
using Sufong2001.Comm.Interfaces;
using Sufong2001.Comm.Models.Events;
using Sufong2001.Comm.Models.Storage;
using Sufong2001.Comm.Models.Storage.Partitions;
using Sufong2001.Share.AzureStorage;
using Sufong2001.Share.Json;
using System.Linq;
using System.Threading.Tasks;

namespace Sufong2001.Comm.AzureFunctions.ServProcesses
{
    [DependencyInjectionConfig(typeof(DiConfig))]
    public static class ProcessFunctions
    {
        [FunctionName(ServiceNames.ProcessStarter)]
        public static async Task ProcessStarter(
            [QueueTrigger(QueueNames.CommProcess)] UploadCompleted uploadCompleted,
            [OrchestrationClient] DurableOrchestrationClient starter,
            ILogger log)
        {
            var orchestrationId = await starter.StartNewAsync(OrchestratorNames.ProcessMessage, uploadCompleted);

            log.Log(LogLevel.Information,
                $"Started an orchestration {orchestrationId} for uploaded manifest {uploadCompleted.SessionId}");
        }

        /// <summary>
        /// 1. Get the first segment of the scheduled messages.
        /// 2. mark they as in progress
        /// 3. push to send queue
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="scheduleTable"></param>
        /// <param name="sendQueue"></param>
        /// <param name="idGenerator"></param>
        /// <param name="app"></param>
        /// <param name="log"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [FunctionName(ServiceNames.Scheduler)]
        public static async Task Scheduler(
            [TimerTrigger("0 */1 * * * *")] TimerInfo timer, // every minutes
            [Table(TableNames.CommSchedule)] CloudTable scheduleTable,
            [Queue(QueueNames.CommSend)] CloudQueue sendQueue,
            [Inject] IScheduleIdGenerator idGenerator,
            [Inject] App app,
            ILogger log,
            ExecutionContext context
            )
        {
            var timestamp = timer.ScheduleStatus.Last;
            var rowRange = timestamp.ToString("yyyyMMdd<");

            // 1. Get the first segment of the scheduled messages.
            var exec = await scheduleTable.GetFirstSegmentOf<MessageSchedule>(CommSchedulePartitionKeys.Scheduled, rowRange);

            if (exec.Results.Count == 0) return;

            var schedules = exec.Results;

            // TODO: can do additional schedule filter here if it is required
            // var delayed = await schedules.MoveTo(scheduleTable
            //         , s => CommSchedulePartitionKeys.Scheduled
            //         , s => idGenerator.ScheduleId(timestamp, "!")
            //     );
            //  
            // return;

            // 2. mark they as in progress
            var results = await schedules.MoveTo(scheduleTable, schedule => CommSchedulePartitionKeys.InProgress);

            // 3. push to send queue
            var queue = results.Select(e =>
            {
                var dispatch = e.OriginalEntity.IsOrMap<MessageDispatch>();
                dispatch.RowKey = e.RowKey;
                return dispatch.AddMessageToAsync(sendQueue);
            }).ToArray();

            await Task.WhenAll(queue);
        }

        /// <summary>
        /// Redistribute the sending message to associated type message queue accordingly.
        /// This act as a shortcut to dispatch message send out to different queue by the type. Such as sms, email and postage
        /// </summary>
        /// <param name="dispatch"></param>
        /// <param name="deliveryQueue"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(ServiceNames.Dispatcher)]
        public static async Task Dispatcher(
            [QueueTrigger(QueueNames.CommSend)] MessageDispatch dispatch,
            [Queue("{QueueName}")] CloudQueue deliveryQueue,
            ILogger log)
        {
            await dispatch.AddMessageToAsync(deliveryQueue);
        }
    }
}