using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Sufong2001.Accounting.Api.Functions.Authorization.Names;
using Sufong2001.Core.Storage.Interfaces;
using Sufong2001.Share.AzureStorage;
using Sufong2001.Share.Json;
using Xero.NetStandard.OAuth2.Token;

namespace Sufong2001.Accounting.Api.Functions.Authorization.Token
{
    public class TokenTable : ITokenStore
    {
        private readonly CloudTable _table;

        public TokenTable(ITableRepository storgeRepository)
        {
            _table = storgeRepository.GetTable(nameof(ContainerName.AccoTokens));
        }

        public async Task StoreToken(XeroOAuth2Token xeroToken)
        {
            var records = xeroToken.Tenants
                .Select(t =>
                {
                    var token = xeroToken.IsOrMap<Functions.Authorization.Token.Token>();
                    token.Tenant = t; // new[] { t }.ToList(); // reset to store only one tenant details

                    return token.CreatTableEntity(nameof(PartitionKeyValue.Xero), t.TenantId.ToString());
                });

            await records.UpdateIn(_table);
        }

        public async Task<XeroOAuth2Token> GetStoredToken(string tenantId = null)
        {
            // strategy 1 by query
            async Task<Functions.Authorization.Token.Token> GetTheFirstOne()
            {
                var query = _table.CreateQuery<TableEntityAdapter<Functions.Authorization.Token.Token>>()
                    .Where(r => r.PartitionKey == nameof(PartitionKeyValue.Xero))
                    .AsTableQuery();

                var resp = await _table.ExecuteQuerySegmentedAsync(query, null);

                return resp.Results.FirstOrDefault()?.OriginalEntity;
            }

            // strategy 2 by read item
            async Task<Functions.Authorization.Token.Token> GetByTenantId()
            {
                var resp = await _table.Retrieve<Functions.Authorization.Token.Token>(nameof(PartitionKeyValue.Xero), tenantId);

                return resp;
            }

            var result = tenantId == null ? await GetTheFirstOne() : await GetByTenantId();

            if (result == null) return null;

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