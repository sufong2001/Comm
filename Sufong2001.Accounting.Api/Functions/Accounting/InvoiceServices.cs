using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Sufong2001.Accounting.Api.Functions.Webhooks.Names;
using Sufong2001.Accounting.Api.Storage.Names;
using Sufong2001.Core.Storage.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sufong2001.Accounting.Api.Functions.Accounting.Names;
using Sufong2001.Accounting.Api.Storage.Model;
using Sufong2001.Share.Json;

namespace Sufong2001.Accounting.Api.Functions.Accounting
{
    public class InvoiceServices
    {
        private readonly IStorageRepository _storageRepository;

        public InvoiceServices(IStorageRepository storageRepository)
        {
            _storageRepository = storageRepository;
        }

        [FunctionName("InvoiceListSync")]
        public async Task InvoiceListSync(
            [CosmosDBTrigger(nameof(DatabaseName.Sufong2001), nameof(WebhookContainerName.WebhookPayloads), ConnectionStringSetting = DatabaseConfig.CosmosDbConnectionString
                , LeaseCollectionName = nameof(InvoiceContainerName.InvoiceLeases), CreateLeaseCollectionIfNotExists = true)]
                IReadOnlyList<Document> list
            , [CosmosDB(nameof(DatabaseName.Sufong2001), nameof(InvoiceContainerName.DataList), ConnectionStringSetting = DatabaseConfig.CosmosDbConnectionString)] 
                IAsyncCollector<InvoiceEntity> invoicesOut

            , ILogger log
                )
        {
            var operations = list.Select(i =>
            {
                var inv = i.IsOrMap<InvoiceEntity>();
                inv.Pk = "INVOICE";
                return invoicesOut.AddAsync(inv);
            });

            await Task.WhenAll(operations);
        }
    }
}