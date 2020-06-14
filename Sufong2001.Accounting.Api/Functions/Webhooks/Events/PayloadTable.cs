using Microsoft.Azure.Cosmos.Table;
using Sufong2001.Accounting.Api.Functions.Webhooks.Names;
using Sufong2001.Core.Storage.Interfaces;
using Sufong2001.Share.AzureStorage;
using System;
using System.Threading.Tasks;

namespace Sufong2001.Accounting.Api.Functions.Webhooks.Events
{
    public class PayloadTable
    {
        private readonly CloudTable _table;

        public PayloadTable(ITableRepository storgeRepository)
        {
            _table = storgeRepository.GetTable(nameof(WebhookContainerName.WebhookPayloads));
        }

        public async Task<(string partitionKey, string rowkey)> Store(string payload)
        {
            var item = new PayloadContent
            {
                Payload = payload
            };

            var k = (partitionKey : nameof(PartitionKeyValue.New), rowkey : Guid.NewGuid().ToString());

            await item.CreateIn(_table, k.partitionKey, k.rowkey);

            return k;
        }
    }

    public class PayloadContent
    {
        public string Payload { get; set; }
    }
}