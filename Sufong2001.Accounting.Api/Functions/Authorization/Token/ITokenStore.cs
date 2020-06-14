using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Token;

namespace Sufong2001.Accounting.Api.Functions.Authorization.Token
{
    public interface ITokenStore
    {
        Task StoreToken(XeroOAuth2Token xeroToken);

        //Task<XeroOAuth2Token> GetStoredToken(string tenantId = null);

        Task<XeroOAuth2Token> GetStoredToken(string tenantId = null);

        Task<bool> TokenExists();

        Task DestroyToken();
    }
}