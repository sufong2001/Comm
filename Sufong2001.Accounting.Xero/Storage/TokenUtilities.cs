using Newtonsoft.Json;
using System.IO;
using Xero.NetStandard.OAuth2.Token;

namespace Sufong2001.Accounting.Xero.Storage
{
    public class TokenStorage
    {
        private readonly string _serializedXeroTokenPath = "./xerotoken.json";

        public void StoreToken(XeroOAuth2Token xeroToken)
        {
            var serializedXeroToken = JsonConvert.SerializeObject(xeroToken);
            System.IO.File.WriteAllText(_serializedXeroTokenPath, serializedXeroToken);
        }

        public XeroOAuth2Token GetStoredToken()
        {
            var serializedXeroToken = File.ReadAllText(_serializedXeroTokenPath);
            var xeroToken = JsonConvert.DeserializeObject<XeroOAuth2Token>(serializedXeroToken);

            return xeroToken;
        }

        public bool TokenExists()
        {
            var fileExist = File.Exists(_serializedXeroTokenPath);

            return fileExist;
        }

        public void DestroyToken()
        {
            File.Delete(_serializedXeroTokenPath);
        }
    }
}