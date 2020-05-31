using AzureFunctions.Autofac;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Sufong2001.Comm.AzureFunctions.Names;
using Sufong2001.Comm.AzureStorage.Interfaces;
using Sufong2001.Comm.AzureStorage.Names;
using Sufong2001.Comm.BusinessEntities;
using Sufong2001.Comm.Configurations.Resolvers;
using Sufong2001.Comm.Models.Events;
using Sufong2001.Comm.Models.Storage;
using Sufong2001.Comm.Models.Storage.Partitions;
using Sufong2001.Share.AzureStorage;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Sufong2001.Comm.AzureFunctions.ServProcesses
{
    public class ProcessFunctions
    {

        private readonly ICommRepository _commRepository;
        private readonly IQueueRepository _queueRepository;

        public ProcessFunctions(ICommRepository commRepository, IQueueRepository queueRepository)
        {
            _commRepository = commRepository ?? throw new ArgumentNullException(nameof(commRepository));
            _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
        }


        [FunctionName(ServiceNames.ProcessStarter)]
        public async Task ProcessStarter(
            [QueueTrigger(QueueNames.CommProcess)] UploadCompleted uploadCompleted,
            [DurableClient] IDurableOrchestrationClient starter,
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
        /// <param name="queueRepository"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(ServiceNames.Scheduler)]
        public async Task Scheduler(
            [TimerTrigger("0 */1 * * * *")] TimerInfo timer, // every minutes
            [Table(TableNames.CommSchedule)] CloudTable scheduleTable,
            ILogger log)
        {

            var rowRange = timer.ResolveRowRange();

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
            await results.DispatchTo(_queueRepository);
        }

        /// <summary>
        /// Redistribute the sending message to associated type message queue accordingly.
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="scheduleTable"></param>
        /// <param name="queueRepository"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(ServiceNames.Dispatcher)]
        public async Task Dispatcher(
            [TimerTrigger("0 */1 * * * *")] TimerInfo timer, // every minutes
            [Table(TableNames.CommSchedule)] CloudTable scheduleTable,
            ILogger log)
        {
            var rowRange = timer.ResolveRowRange();

            // 1. Get the first segment of the scheduled messages.
            var exec = await scheduleTable.GetFirstSegmentOf<MessageSchedule>(CommSchedulePartitionKeys.InProgress, rowRange);

            if (exec.Results.Count == 0) return;

            await exec.Results.DispatchTo(_queueRepository);
        }
    }
}