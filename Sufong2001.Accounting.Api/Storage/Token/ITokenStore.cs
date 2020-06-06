using Xero.NetStandard.OAuth2.Token;

namespace Sufong2001.Accounting.Api.Storage.Token
{
    public interface ITokenStore
    {
        void StoreToken(XeroOAuth2Token xeroToken);

        //Task<XeroOAuth2Token> GetStoredToken(string tenantId = null);

        XeroOAuth2Token GetStoredToken(string tenantId = null);

        bool TokenExists();

        void DestroyToken();
    }
}