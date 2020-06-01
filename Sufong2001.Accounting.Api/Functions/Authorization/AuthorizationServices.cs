using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Sufong2001.Accounting.Api.Functions.Authorization.Names;
using Sufong2001.Accounting.Xero.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Models;
using Xero.NetStandard.OAuth2.Token;

namespace Sufong2001.Accounting.Api.Functions.Authorization
{
    public class AuthorizationServices
    {
        private readonly XeroClient _client;
        private readonly TokenStorage _tokenStorage;

        public AuthorizationServices(XeroClient client, TokenStorage tokenStorage)
        {
            _client = client;
            _tokenStorage = tokenStorage;
        }

        [FunctionName(nameof(AuthorizationServiceNames.Authorization))]
        public IActionResult Authorization(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req
            , ILogger log
            )
        {
            return new RedirectResult(_client.BuildLoginUri());
        }

        [FunctionName(nameof(AuthorizationServiceNames.Callback))]
        public async Task<IActionResult> CallbackAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Authorization/Callback")] HttpRequest req
            , ILogger log
        )
        {
            var code = req.Query["code"];

            var xeroToken = (XeroOAuth2Token)await _client.RequestXeroTokenAsync(code);

            List<Tenant> tenants = await _client.GetConnectionsAsync(xeroToken);

            Tenant firstTenant = tenants[0];

            _tokenStorage.StoreToken(xeroToken);

            return new OkResult();
        }

        [FunctionName(nameof(AuthorizationServiceNames.Connection))]
        public async Task<IActionResult> ConnectionAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Authorization/Connection")] HttpRequest req
            , ILogger log
        )
        {
            var xeroToken = _tokenStorage.GetStoredToken();

            var tenants = await _client.GetConnectionsAsync(xeroToken);

            return new JsonResult(tenants);
        }

        [FunctionName(nameof(AuthorizationServiceNames.Disconnect))]
        public async Task<IActionResult> DiconnectAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Authorization/Disconnect")] HttpRequest req
            , ILogger log
        )
        {
            var xeroToken = _tokenStorage.GetStoredToken();
            var xeroTenant = xeroToken.Tenants[0];

            await _client.DeleteConnectionAsync(xeroToken, xeroTenant);

            _tokenStorage.DestroyToken();

            return new JsonResult(xeroTenant);
        }
    }
}