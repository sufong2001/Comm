using AzureFunctions.Autofac;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Sufong2001.Comm.AzureFunctions.Names;
using Sufong2001.Comm.AzureStorage.Names;
using Sufong2001.Comm.BusinessEntities;
using Sufong2001.Comm.Configurations.Resolvers;
using Sufong2001.Comm.Dto.Interfaces;
using Sufong2001.Comm.Dto.Messages;
using Sufong2001.Comm.Interfaces;
using Sufong2001.Comm.Models.Events;
using Sufong2001.Comm.Models.Storage;
using Sufong2001.Comm.Models.Storage.Partitions;
using Sufong2001.Share.AzureStorage;
using Sufong2001.Share.Json;
using System.Threading.Tasks;

namespace Sufong2001.Comm.AzureFunctions.ServOuts
{
    [DependencyInjectionConfig(typeof(CommConfig))]
    public static class DeliveryFunctions
    {
        [FunctionName(ServiceNames.DeliverySms)]
        public static async Task DeliverySms(
            [QueueTrigger(QueueNames.CommSendSms)] MessageDispatch sendMessage,
            [Table(TableNames.CommSchedule, CommSchedulePartitionKeys.InProgress, "{RowKey}")] TableEntityAdapter<MessageSchedule> schedule,
            [Table(TableNames.CommMessage, CommMessagePartitionKeys.Created, "{MessageReference}")] TableEntityAdapter<Message> message,
            [Table(TableNames.CommSchedule)] CloudTable scheduleTable,
            [Inject] App app,
            [Inject] IScheduleIdGenerator idGenerator,
            ILogger log)
        {
            if (schedule == null) return;

            var sms = message.GetMessageOf<Sms>();

            // handle unexpected non existing message
            if (sms == null)
            {
                await schedule.MoveTo(scheduleTable, s => CommSchedulePartitionKeys.Skipped);
                return;
            }

            await schedule.MarkScheduleSent(scheduleTable, app, idGenerator);
        }

        [FunctionName(ServiceNames.DeliveryEmail)]
        public static async Task DeliveryEmail(
            [QueueTrigger(QueueNames.CommSendEmail)] MessageDispatch sendMessage,
            [Table(TableNames.CommSchedule, CommSchedulePartitionKeys.InProgress, "{RowKey}")] TableEntityAdapter<MessageSchedule> schedule,
            [Table(TableNames.CommMessage, CommMessagePartitionKeys.Created, "{MessageReference}")] TableEntityAdapter<Message> message,
            [Table(TableNames.CommSchedule)] CloudTable scheduleTable,
            [Inject] App app,
            [Inject] IScheduleIdGenerator idGenerator,
            ILogger log)
        {
            if (schedule == null) return;

            var email = message.GetMessageOf<Email>();

            // handle unexpected non existing message
            if (email == null)
            {
                await schedule.MoveTo(scheduleTable, s => CommSchedulePartitionKeys.Skipped).ConfigureAwait(false);
                return;
            }

            await schedule.MarkScheduleSent(scheduleTable, app, idGenerator);
        }

        [FunctionName(ServiceNames.DeliveryPostage)]
        public static async Task DeliveryPostage(
            [QueueTrigger(QueueNames.CommSendPostage)] MessageDispatch sendMessage,
            [Table(TableNames.CommSchedule, CommSchedulePartitionKeys.InProgress, "{RowKey}")] TableEntityAdapter<MessageSchedule> schedule,
            [Table(TableNames.CommMessage, CommMessagePartitionKeys.Created, "{MessageReference}")] TableEntityAdapter<Message> message,
            [Table(TableNames.CommSchedule)] CloudTable scheduleTable,
            [Inject] App app,
            [Inject] IScheduleIdGenerator idGenerator,
            ILogger log)
        {
            if (schedule == null) return;

            var postage = message.GetMessageOf<Postage>();

            // handle unexpected non existing message
            if (postage == null)
            {
                await schedule.MoveTo(scheduleTable, s => CommSchedulePartitionKeys.Skipped).ConfigureAwait(false);
                return;
            }

            await schedule.MarkScheduleSent(scheduleTable, app, idGenerator);
        }

        private static T GetMessageOf<T>(this TableEntityAdapter<Message> message) where T : IMessage
        {
            if (message == null) return default;

            return message.OriginalEntity.IsOrMap<T>();
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