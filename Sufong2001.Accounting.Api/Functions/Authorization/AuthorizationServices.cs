using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Sufong2001.Accounting.Api.Functions.Authorization.Names;
using Sufong2001.Accounting.Api.Functions.Authorization.Token;
using System.Linq;
using System.Threading.Tasks;
using Sufong2001.Accounting.Xero.Authorization;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Token;

namespace Sufong2001.Accounting.Api.Functions.Authorization
{
    public class AuthorizationServices : IAzFunc
    {
        private readonly XeroClient _client;
        private readonly ITokenStore _tokenStore;

        public AuthorizationServices(XeroClient client, ITokenStore tokenStore)
        {
            _client = client;
            _tokenStore = tokenStore;
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

            await _tokenStore.StoreToken(xeroToken);

            return new OkResult();
        }

        [FunctionName(nameof(AuthorizationServiceNames.Connection))]
        public async Task<IActionResult> ConnectionAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Authorization/Connection")] HttpRequest req
            , ILogger log
        )
        {
            var xeroToken = await _tokenStore.GetStoredToken();

            var tenants = await _client.GetConnectionsAsync(xeroToken);

            return new JsonResult(tenants);
        }

        [FunctionName(nameof(AuthorizationServiceNames.Refresh))]
        public async Task<IActionResult> RefreshAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Authorization/Refresh")] HttpRequest req
            , ILogger log
        )
        {
            var xeroToken = await _tokenStore.GetStoredToken();

            xeroToken = (XeroOAuth2Token)await _client.RefreshAccessTokenAsync(xeroToken);

            await _tokenStore.StoreToken(xeroToken);

            return new JsonResult(xeroToken);
        }

        [FunctionName(nameof(AuthorizationServiceNames.Disconnect))]
        public async Task<IActionResult> DiconnectAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Authorization/Disconnect")] HttpRequest req
            , ILogger log
        )
        {
            var xeroToken = await _tokenStore.GetStoredToken("");

            var xeroTenant = xeroToken.Tenants.First();
            await _client.DeleteConnectionAsync(xeroToken, xeroTenant);

            await _tokenStore.DestroyToken();

            return new JsonResult(xeroTenant);
        }
    }
}