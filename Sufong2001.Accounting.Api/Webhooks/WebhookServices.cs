using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Sufong2001.Accounting.Api.Webhooks.Names;
using Sufong2001.Accounting.Xero.Webhooks;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sufong2001.Accounting.Api.Webhooks
{
    public class WebhookServices

    {
        //private readonly HttpClient _client;
        private readonly ISignatureVerifier _signatureVerifier;

        public WebhookServices(ISignatureVerifier signatureVerifier)
        {
            //_client = httpClient;
            _signatureVerifier = signatureVerifier;
        }

        [FunctionName(nameof(WebhookServiceNames.XeroWebhooks))]
        public async Task<IActionResult> XeroWebhooksAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req
            , ILogger log
            )
        {
            try
            {
                var payload = await req.Body.Content();

                var signatureHeader = req.Headers["x-xero-signature"].FirstOrDefault();

                var isValid = _signatureVerifier.VerifySignature(payload, signatureHeader);

                if (!isValid) return new UnauthorizedResult();

                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex.StackTrace);
                return new UnauthorizedResult();
            }
        }
    }
}