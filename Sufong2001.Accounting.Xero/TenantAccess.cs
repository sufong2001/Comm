using Sufong2001.Accounting.Xero.Storage;
using System;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Token;

namespace Sufong2001.Accounting.Xero
{
    public class TenantAccess
    {
        public XeroClient Client { get; }
        public TokenStorage TokenStorage { get; }

        public TenantAccess(XeroClient client, TokenStorage tokenStorage)
        {
            Client = client;
            TokenStorage = tokenStorage;
        }

        public async Task<(string accessToken, string tenantId)> GetAccessToken()
        {
            var xeroToken = TokenStorage.GetStoredToken();
            var utcTimeNow = DateTime.UtcNow;

            if (utcTimeNow > xeroToken.ExpiresAtUtc)
            {
                xeroToken = (XeroOAuth2Token)await Client.RefreshAccessTokenAsync(xeroToken);
                TokenStorage.StoreToken(xeroToken);
            }

            var accessToken = xeroToken.AccessToken;
            var xeroTenantId = xeroToken.Tenants[0].TenantId.ToString();

            return (accessToken, xeroTenantId);
        }
    }
}