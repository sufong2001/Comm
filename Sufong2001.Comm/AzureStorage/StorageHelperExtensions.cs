using Microsoft.WindowsAzure.Storage.Table;
using Sufong2001.Share.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sufong2001.Comm.Dto;
using Sufong2001.Comm.Models.Storage;
using Sufong2001.Share.Arrays;
using Sufong2001.Share.String;

namespace Sufong2001.Comm.AzureStorage
{
    public static class StorageHelperExtensions
    {
        public static IEnumerable<ITableEntity> PrepareCommMessage(this CommunicationManifest cm, string manifestKey)
        {
            var entities = new (string key, object message)[]
            {
                ("Sms"    ,  cm.Sms    ),
                ("Email"  ,  cm.Email  ),
                ("Postage",  cm.Postage),

            }.Select((entity, i) =>
            {
                var m = entity.message;
                var k = entity.key;

                var e = m.JClone<Message>();

                e.Reference = cm.Reference;
                e.Title = cm.Title;

                if (m is Dto.Interfaces.IAttachments att)
                {
                    e.AttachmentList = new[] { cm.Attachments, att.Attachments }
                        .Merge().ArrayToString(Environment.NewLine);
                }

                return new TableEntityAdapter<Message>(e, manifestKey, $"{manifestKey}_{k}-{i}");
            });

            return entities;
        }
    }
}