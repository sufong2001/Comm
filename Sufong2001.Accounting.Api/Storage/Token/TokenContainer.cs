using Microsoft.Azure.Cosmos;
using Sufong2001.Accounting.Api.Storage.Token.Names;
using Sufong2001.Core.Storage.Interfaces;
using Sufong2001.Share.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Token;
using QueryRequestOptions = Microsoft.Azure.Cosmos.QueryRequestOptions;

namespace Sufong2001.Accounting.Api.Storage.Token
{
    public class TokenContainer : ITokenStore
    {
        private readonly Container _container;

        public TokenContainer(ICosmosDbRepository storgeRepository)
        {
            _container = storgeRepository.GetContainer(nameof(ContainerName.AccoTokens));
        }

        public async Task StoreToken(XeroOAuth2Token xeroToken)
        {
            var operations = xeroToken.Tenants
                .Select(t =>
                {
                    var token = xeroToken.IsOrMap<Token>();
                    token.Tenant = t; // new[] { t }.ToList(); // reset to store only one tenant details
                    token.Id = t.TenantId.ToString();
                    token.Pk = nameof(PartitionKeyValue.Xero);

                    return _container.UpsertItemAsync(token);
                });

            await Task.WhenAll(operations);

            //await _container.CreateItemAsync(xeroToken, new PartitionKey("xero"));
        }

        public async Task<XeroOAuth2Token> GetStoredToken(string tenantId = null)
        {
            var partitionKey = new PartitionKey(nameof(PartitionKeyValue.Xero));

            // strategy 1 by query
            async Task<Token> GetTheFirstOne()
            {
                var queryRequestOptions = new QueryRequestOptions()
                {
                    PartitionKey = partitionKey,
                    MaxConcurrency = 1
                };

                var query = _container.GetItemQueryIterator<Token>(
                    "SELECT top 1 * FROM c",
                    requestOptions: queryRequestOptions);

                var feed = await query.ReadNextAsync();

                return feed.FirstOrDefault();
            }

            // strategy 2 by read item
            async Task<Token> GetByTenantId()
            {
                var resp = await _container.ReadItemAsync<Token>(tenantId, partitionKey);

                return resp;
            }

            var result = tenantId == null ? await GetTheFirstOne() : await GetByTenantId();

            if (result == null) return null;

            // remap to XeroOAuth2Token object
            var xeroToken = result.IsOrMap<XeroOAuth2Token>();
            xeroToken.Tenants = new[] { result.Tenant }.ToList();

            return xeroToken;
        }

        public Task<bool> TokenExists()
        {
            throw new NotImplementedException();
        }

        public Task DestroyToken()
        {
            throw new NotImplementedException();
        }
    }
}