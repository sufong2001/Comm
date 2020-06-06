using Microsoft.Azure.Cosmos;
using Sufong2001.Core.Storage.Interfaces;
using Sufong2001.Share.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Sufong2001.Accounting.Api.Storage.Token.Names;
using Xero.NetStandard.OAuth2.Token;
using PartitionKey = Sufong2001.Accounting.Api.Storage.Token.Names.PartitionKey;

namespace Sufong2001.Accounting.Api.Storage.Token
{
    public class TokenContainer : ITokenStore
    {
        private readonly Container _container;

        public TokenContainer(ICosmosDbRepository storgeRepository)
        {
            _container = storgeRepository.GetContainer(nameof(ContainerName.AccoTokens));
        }

        public async void StoreToken(XeroOAuth2Token xeroToken)
        {
            var operations = xeroToken.Tenants
                .Select(t =>
                {
                    var token = xeroToken.IsOrMap<Token>();
                    token.Tenant = t; // new[] { t }.ToList(); // reset to store only one tenant details
                    token.Id = t.TenantId.ToString();
                    token.Pk = nameof(PartitionValue.Xero);

                    return _container.UpsertItemAsync(token);
                });

            await Task.WhenAll(operations);

            //await _container.CreateItemAsync(xeroToken, new PartitionKey("xero"));
        }

        public XeroOAuth2Token GetStoredToken(string tenantId = null)
        {
            var queryRequestOptions = new QueryRequestOptions();

            var query = _container.GetItemQueryIterator<Token>(
                requestOptions: queryRequestOptions);

            //if (tenantId != null)
            //{
            //    query = (IOrderedQueryable<Token>)query.Where(x => x.Tenant.TenantId.ToString() == tenantId);
            //}

            var result = query.ReadNextAsync().Result.FirstOrDefault();

            if (result == null) return null;

            var xeroToken = result.IsOrMap<XeroOAuth2Token>();
            xeroToken.Tenants = new[] { result.Tenant }.ToList();

            return xeroToken;
        }

        public bool TokenExists()
        {
            throw new NotImplementedException();
        }

        public void DestroyToken()
        {
            throw new NotImplementedException();
        }
    }
}