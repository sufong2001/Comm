using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Token;

namespace Sufong2001.Accounting.Xero.Authorization
{
    public class TokenStorage : ITokenStore
    {
        private readonly string _serializedXeroTokenPath = "./xerotoken.json";

        public async Task StoreToken(XeroOAuth2Token xeroToken)
        {
            await Task.Run(() =>
            {
                var serializedXeroToken = JsonConvert.SerializeObject(xeroToken);
                File.WriteAllText(_serializedXeroTokenPath, serializedXeroToken);
            });
        }

        public async Task<XeroOAuth2Token> GetStoredToken(string tenantId = null)
        {
            var serializedXeroToken = await File.ReadAllTextAsync(_serializedXeroTokenPath);
            var xeroToken = JsonConvert.DeserializeObject<XeroOAuth2Token>(serializedXeroToken);

            return await Task.FromResult(xeroToken);
        }

        public async Task<bool> TokenExists()
        {
            var fileExist = File.Exists(_serializedXeroTokenPath);

            return await Task.FromResult(fileExist); ;
        }

        public async Task DestroyToken()
        {
            await Task.Run(() =>
            {
                File.Delete(_serializedXeroTokenPath);
            });
        }
    }
}