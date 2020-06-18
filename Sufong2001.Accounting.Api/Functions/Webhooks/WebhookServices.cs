using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Sufong2001.Accounting.Api.Functions.Webhooks.Events;
using Sufong2001.Accounting.Api.Functions.Webhooks.Names;
using Sufong2001.Accounting.Api.Storage.Model;
using Sufong2001.Accounting.Api.Storage.Names;
using Sufong2001.Accounting.Xero;
using Sufong2001.Accounting.Xero.Authorization;
using Sufong2001.Accounting.Xero.Webhooks;
using Sufong2001.Accounting.Xero.Webhooks.Models;
using Sufong2001.Share.AzureStorage;
using Sufong2001.Share.IO;
using Sufong2001.Share.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;

namespace Sufong2001.Accounting.Api.Functions.Webhooks
{
    public class WebhookServices : IAzFunc
    {
        private readonly ISignatureVerifier _signatureVerifier;
        private readonly PayloadTable _storeTable;
        private readonly ITokenStore _tokenStore;
        private readonly AccountingApi _accountingApi;
        private TenantAccess _tenantAccess;

        public WebhookServices(ISignatureVerifier signatureVerifier
            , PayloadTable storeTable, ITokenStore tokenStore, TenantAccess tenantAccess
            , AccountingApi accountingApi)
        {
            //_client = httpClient;
            _signatureVerifier = signatureVerifier;
            _storeTable = storeTable;
            _tokenStore = tokenStore;
            _accountingApi = accountingApi;
            _tenantAccess = tenantAccess;
        }

        [FunctionName("WebhookPayloadGenerator")]
        public async Task<IActionResult> WebhookPayloadGenerator(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "XeroWebhooks/WebhookPayloadGenerator")] HttpRequest req
            , [Queue(WebhookQueueNames.ProcessWebhookEvents)] CloudQueue processQueue
            , ILogger log
        )
        {
            var (accessToken, xeroTenantId) = await _tenantAccess.GetAccessToken();

            var sevenDaysAgo = DateTime.Now.AddDays(-30).ToString("yyyy, MM, dd");
            var invoicesFilter = "Date >= DateTime(" + sevenDaysAgo + ")";

            var response = await _accountingApi.GetInvoicesAsync(accessToken, xeroTenantId, null, invoicesFilter);
            var events = response._Invoices.Select(i => new Event()
            {
                TenantId = Guid.Parse(xeroTenantId),
                ResourceId = i?.InvoiceID ?? Guid.NewGuid(),
                EventType = "Update",
                EventCategory = "INVOICE",
                EventDateUtc = i?.UpdatedDateUTC ?? DateTime.Now.ToUniversalTime(),
                ResourceUrl = i?.Url,
                TenantType = "ORGANISATION",
            }).ToList();

            var keys = await _storeTable.Store(new Payload
            {
                Events = events,
                Entropy = "test",
                LastEventSequence = events.Count(),
                FirstEventSequence = 1
            }.ToJson());

            // queue and trigger WebhookServiceNames.ProcessWebhookEvents
            await keys.AddMessageToAsync(processQueue);

            return new JsonResult(events);
        }

        /// <summary>
        /// The web hook to receive event from Xero
        /// </summary>
        /// <param name="req"></param>
        /// <param name="processQueue"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(nameof(WebhookServiceNames.XeroWebhooks))]
        public async Task<IActionResult> XeroWebhooksAsync(
              [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req
            , [Queue(WebhookQueueNames.ProcessWebhookEvents)] CloudQueue processQueue
            , ILogger log
            )
        {
            try
            {
                var (signatureHeader, payload) = (
                    req.Headers["x-xero-signature"].FirstOrDefault(),
                    await req.Body.Content()
                );

                var isValid = _signatureVerifier.VerifySignature(payload, signatureHeader);

                if (!isValid) return new UnauthorizedResult();

                // store the payload into local storage
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

        /// <summary>
        /// Process the web hook events
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="context"></param>
        /// <param name="invoicesOut"></param>
        /// <param name="payloadTable"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(nameof(WebhookServiceNames.ProcessWebhookEvents))]
        public async Task ProcessWebhookEvents(
            [QueueTrigger(WebhookQueueNames.ProcessWebhookEvents)] Tuple<string, string> keys,
            [Table(nameof(WebhookContainerName.WebhookPayloads), nameof(PartitionKeyValue.New), "{Item2}")] TableEntityAdapter<PayloadContent> context,
            [CosmosDB(nameof(DatabaseName.Sufong2001), nameof(WebhookContainerName.WebhookPayloads), ConnectionStringSetting = DatabaseConfig.CosmosDbConnectionString)] IAsyncCollector<InvoiceEntity> invoicesOut,
            [Table(nameof(WebhookContainerName.WebhookPayloads))] CloudTable payloadTable,
            ILogger log)
        {
            if (context == null) return;

            var getInvoices = context.OriginalEntity.Payload.To<Payload>().Events
                .Where(e => e.EventCategory == "INVOICE")
                .GroupBy(e => e.TenantId)
                .Select(GetInvoicesAsync);

            // Get Invoice details from Xero
            var result = await Task.WhenAll(getInvoices);

            var invoices = result.SelectMany(i => i);

            var operations = invoices.Select(i => invoicesOut.AddAsync(i));

            await Task.WhenAll(operations);

            await context.MoveTo(payloadTable, p => nameof(PartitionKeyValue.Processed));
        }

        /// <summary>
        /// Get Invoice details from Xero and convert them to Cosmos DB document entity
        /// </summary>
        /// <param name="eg"></param>
        /// <returns></returns>
        private async Task<IEnumerable<InvoiceEntity>> GetInvoicesAsync(IGrouping<Guid, Event> eg)
        {
            var tenantId = eg.Key.ToString();
            var token = await _tokenStore.GetStoredToken(tenantId);
            var invoiceIds = eg.Select(e => e.ResourceId).ToList();

            var response = await _accountingApi.GetInvoicesAsync(token.AccessToken, tenantId, iDs: invoiceIds);
            return response._Invoices.Select(i =>
            {
                var inv = i.IsOrMap<InvoiceEntity>();
                inv.Id = i.InvoiceID?.ToString();
                inv.Pk = tenantId;

                return inv;
            });
        }
    }
}