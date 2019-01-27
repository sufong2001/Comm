using Microsoft.WindowsAzure.Storage.Table;
using Sufong2001.Comm.Dto;
using Sufong2001.Comm.Models.Storage;
using Sufong2001.Share.Arrays;
using Sufong2001.Share.Json;
using Sufong2001.Share.String;
using System;
using System.Collections.Generic;
using System.Linq;
using Sufong2001.Comm.Interfaces;

namespace Sufong2001.Comm.AzureStorage
{
    public static class StorageHelperExtensions
    {
        public static IEnumerable<ITableEntity> PrepareCommMessage(this CommunicationManifest cm, string sessionId, IMessageIdGenerator idGenerator)
        {
            // ParseMessage function
            (string type, Recipient recipient, object message)[] ParseMessage(Recipient r)
            {
                return new (string type, Recipient recipient, object message)[]
                {
                    ("Sms",     r, r.Sms    ),
                    ("Email",   r, r.Email  ),
                    ("Postage", r, r.Postage)
                };
            }

            // CreateTableEntity function
            TableEntityAdapter<Message> CreateTableEntity((string type, Recipient recipient, object message) entity, int i)
            {
                var type      = entity.type;
                var recipient = entity.recipient;
                var message   = entity.message;

                var e                    = message.IsOrMap<Message>();
                e.SessionId              = sessionId;
                e.MessageReference       = idGenerator.MessageId();
                e.CommunicationReference = cm.CommunicationReference;
                e.Title                  = cm.Title;
                e.Type                   = type;

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