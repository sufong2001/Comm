using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Sufong2001.Accounting.Api.Functions.Webhooks.Events;
using Sufong2001.Accounting.Api.Functions.Webhooks.Names;
using Sufong2001.Accounting.Xero.Webhooks;
using Sufong2001.Share.AzureStorage;
using Sufong2001.Share.IO;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sufong2001.Accounting.Api.Functions.Webhooks
{
    public class WebhookServices
    {
        private readonly ISignatureVerifier _signatureVerifier;
        private readonly PayloadTable _storeTable;

        public WebhookServices(ISignatureVerifier signatureVerifier, PayloadTable storeTable)
        {
            //_client = httpClient;
            _signatureVerifier = signatureVerifier;
            _storeTable = storeTable;
        }

        [FunctionName(nameof(WebhookServiceNames.XeroWebhooks))]
        public async Task<IActionResult> XeroWebhooksAsync(
              [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req
            , [Queue(WebhookQueueNames.WebhookEvents)] CloudQueue processQueue
            , ILogger log
            )
        {
            try
            {
                var payload = await req.Body.Content();

                var signatureHeader = req.Headers["x-xero-signature"].FirstOrDefault();

                var isValid = _signatureVerifier.VerifySignature(payload, signatureHeader);

                if (!isValid) return new UnauthorizedResult();

                var keys = await _storeTable.Store(payload);

                // queue and trigger WebhookServiceNames.ProcessWebhookEvents
                await keys.AddMessageToAsync(processQueue);

                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex.StackTrace);
                return new UnauthorizedResult();
            }
        }

        [FunctionName(nameof(WebhookServiceNames.ProcessWebhookEvents))]
        public static void ProcessWebhookEvents(
            [QueueTrigger(WebhookQueueNames.WebhookEvents)] Tuple<string, string> keys,
            [Table(nameof(WebhookContainerName.WebhookPayloads), nameof(PartitionKeyValue.New), "{Item2}")] TableEntityAdapter<PayloadContent> poco,
            ILogger log)
        {
            log.LogInformation($"PK={poco.PartitionKey}, RK={poco.RowKey}, Text={poco.OriginalEntity.Payload}");
        }
    }
}