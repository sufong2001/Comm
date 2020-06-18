using System;
using System.Threading.Tasks;
using Sufong2001.Accounting.Xero.Authorization;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Token;

namespace Sufong2001.Accounting.Xero
{
    public class TenantAccess
    {
        public XeroClient Client { get; }
        public ITokenStore TokenStorage { get; }

        public TenantAccess(XeroClient client, ITokenStore tokenStorage)
        {
            Client = client;
            TokenStorage = tokenStorage;
        }

        public async Task<(string accessToken, string tenantId)> GetAccessToken()
        {
            var xeroToken = await TokenStorage.GetStoredToken();
            var utcTimeNow = DateTime.UtcNow;

            if (utcTimeNow > xeroToken.ExpiresAtUtc)
            {
                xeroToken = (XeroOAuth2Token)await Client.RefreshAccessTokenAsync(xeroToken);
                await TokenStorage.StoreToken(xeroToken);
            }

            var accessToken = xeroToken.AccessToken;
            var xeroTenantId = xeroToken.Tenants[0].TenantId.ToString();

            return (accessToken, xeroTenantId);
        }
    }
}