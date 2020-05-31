using Microsoft.Azure.Cosmos.Table;
using Sufong2001.Comm.Dto;
using Sufong2001.Comm.Interfaces;
using Sufong2001.Comm.Models.Storage;
using Sufong2001.Comm.Models.Storage.Partitions;
using Sufong2001.Share.Arrays;
using Sufong2001.Share.Assembly;
using Sufong2001.Share.Json;
using Sufong2001.Share.String;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sufong2001.Comm.AzureStorage
{
    public static class StorageHelperExtensions
    {
        public static string[] FailoverOptions(this Recipient r, CommunicationManifest communicationManifest)
        {
            var failoverOptions =
                (r.FailoverOptions ?? communicationManifest.FailoverOptions ?? new string[0]).ToArray();
            return failoverOptions;
        }

        public static DateTime DeliverySchedule(this Recipient r, CommunicationManifest communicationManifest, DateTime defaultTo)
        {
            var delivery = r.ScheduleTime ?? communicationManifest.ScheduleTime ?? defaultTo;
            return delivery;
        }

        private static object GetMessageObject(this Recipient recipient, MessageType type)
        {
            return recipient.GetPropertyValue(type.ToString());
        }

        public static IEnumerable<ITableEntity> PrepareCommMessage(this CommunicationManifest cm, string sessionId, IMessageIdGenerator idGenerator, DateTime prepareDateTime)
        {
            // ParseMessage function
            (int order, string type, Recipient recipient, object message)[] ParseMessage(Recipient r)
            {
                var failoverOptions = r.FailoverOptions(cm);

                return Enum.GetNames(typeof(MessageType)).Select(type => (
                        order: failoverOptions.IndexOf(type),
                        type: type,
                        recipient: r,
                        message: r.GetPropertyValue(type)
                    ))
                    .Where(m => m.message != null)
                    .OrderBy(m => m.order)
                    .ToArray();
            }

            // CreateTableEntity function
            TableEntityAdapter<Message> CreateTableEntity((int order, string type, Recipient recipient, object message) entity, int i)
            {
                var (order, type, recipient, message) = entity;

                var e = message.IsOrMap<Message>();
                e.SessionId = sessionId;
                e.CommunicationReference = cm.CommunicationReference;
                e.RecipientReference = recipient.RecipientReference ?? idGenerator.MessageId(sessionId);
                e.MessageReference = idGenerator.MessageId(sessionId);
                e.Title = cm.Title;
                e.Type = type;
                e.FailoverOptions = recipient.FailoverOptions(cm).ArrayToString();
                e.DeliveryOrder = order > 0 ? order : 0;
                e.DeliverySchedule = recipient.DeliverySchedule(cm, prepareDateTime);

                if (message is Dto.Interfaces.IAttachments att)
                {
                    e.AttachmentList = new[] { cm.Attachments, recipient.Attachments, att.Attachments }.Merge().ArrayToString(Environment.NewLine);
                }

                return new TableEntityAdapter<Message>(e, CommMessagePartitionKeys.Created, e.MessageReference);
            }

            var entities = cm.Recipients
                .SelectMany(ParseMessage)
                .Select(CreateTableEntity);

            return entities;
        }
    }
}