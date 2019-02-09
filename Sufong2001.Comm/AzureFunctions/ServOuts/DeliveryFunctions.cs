using AzureFunctions.Autofac;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
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
using System.Threading.Tasks;

namespace Sufong2001.Comm.AzureFunctions.ServOuts
{
    [DependencyInjectionConfig(typeof(DiConfig))]
    public static class DeliveryFunctions
    {
        [FunctionName(ServiceNames.DeliverySms)]
        public static async Task DeliverySms(
            [QueueTrigger(QueueNames.CommSendSms)] MessageDispatch sendMessage,
            [Table(TableNames.CommSchedule, CommSchedulePartitionKeys.InProgress, "{RowKey}")] TableEntityAdapter<MessageSchedule> schedule,
            [Table(TableNames.CommSchedule)] CloudTable scheduleTable,
            [Inject] App app,
            [Inject] IScheduleIdGenerator idGenerator,
            ILogger log)
        {
            await schedule.MarkScheduleSent(scheduleTable, app, idGenerator);
        }

        [FunctionName(ServiceNames.DeliveryEmail)]
        public static async Task DeliveryEmail(
            [QueueTrigger(QueueNames.CommSendEmail)] MessageDispatch sendMessage,
            [Table(TableNames.CommSchedule, CommSchedulePartitionKeys.InProgress, "{RowKey}")] TableEntityAdapter<MessageSchedule> schedule,
            [Table(TableNames.CommSchedule)] CloudTable scheduleTable,
            [Inject] App app,
            [Inject] IScheduleIdGenerator idGenerator,
            ILogger log)
        {
            await schedule.MarkScheduleSent(scheduleTable, app, idGenerator);
        }

        [FunctionName(ServiceNames.DeliveryPostage)]
        public static async Task DeliveryPostage(
            [QueueTrigger(QueueNames.CommSendPostage)] MessageDispatch sendMessage,
            [Table(TableNames.CommSchedule, CommSchedulePartitionKeys.InProgress, "{RowKey}")] TableEntityAdapter<MessageSchedule> schedule,
            [Table(TableNames.CommSchedule)] CloudTable scheduleTable,
            [Inject] App app,
            [Inject] IScheduleIdGenerator idGenerator,
            ILogger log)
        {
            await schedule.MarkScheduleSent(scheduleTable, app, idGenerator);
        }

        private static async Task MarkScheduleSent(this TableEntityAdapter<MessageSchedule> schedule, CloudTable scheduleTable, App app, IScheduleIdGenerator id)
        {
            var timestamp = app.DateTimeNow;
            MessageSchedule UpdateOriginalEntity(MessageSchedule messageSchedule)
            {
                messageSchedule.Delivered = timestamp;
                return messageSchedule;
            }

            await schedule.MoveTo(scheduleTable
                , s => CommSchedulePartitionKeys.Sent
                , s => id.ScheduleId(timestamp)
                , UpdateOriginalEntity
            );
        }
    }
}