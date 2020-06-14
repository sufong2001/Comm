using Xero.NetStandard.OAuth2.Token;

namespace Sufong2001.Accounting.Xero.Authorization
{
    public interface ITokenStorage
    {
        void StoreToken(XeroOAuth2Token xeroToken);
        XeroOAuth2Token GetStoredToken();
        bool TokenExists();
        void DestroyToken();
    }
}