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

        [FunctionName(ServiceNames.Scheduler)]
        public static async Task Scheduler(
            [TimerTrigger("0 */1 * * * *")] TimerInfo timer, // every minutes
            [Table(TableNames.CommSchedule)] CloudTable scheduleTable,
            [Queue(QueueNames.CommSend)] CloudQueue sendQueue,
            [Inject] IMessageIdGenerator idGenerator,
            [Inject] App app,
            ILogger log)
        {
            var rowRange = timer.ScheduleStatus.Last.ToString("yyyyMMdd0");
            var continuationToken = new TableContinuationToken();

            // Create the table query.
            TableQuery<TableEntityAdapter<MessageSchedule>> rangeQuery =
                new TableQuery<TableEntityAdapter<MessageSchedule>>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, CommSchedulePartitionKeys.Scheduled),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, rowRange)
                    )
                );

            var exec = await scheduleTable.ExecuteQuerySegmentedAsync(rangeQuery, continuationToken);

            if (exec.Results.Count == 0) return;

            var schedules = exec.Results;

            // TODO: do additional the schedule filter here

            var results = await schedules.MoveTo(scheduleTable, schedule => CommSchedulePartitionKeys.InProgress);

            var queue = results.Select(e =>
            {
                var dispatch = e.OriginalEntity.IsOrMap<MessageDispatch>();
                dispatch.RowKey = e.RowKey;
                return dispatch.AddMessageToAsync(sendQueue);
            }).ToArray();

            await Task.WhenAll(queue);
        }

        /// <summary>
        /// Redistribute the sending message to separated type message queue
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