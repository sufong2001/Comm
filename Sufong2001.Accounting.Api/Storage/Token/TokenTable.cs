using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Sufong2001.Accounting.Api.Storage.Token.Names;
using Sufong2001.Core.Storage.Interfaces;
using Sufong2001.Share.AzureStorage;
using Sufong2001.Share.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Token;

namespace Sufong2001.Accounting.Api.Storage.Token
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
                    var token = xeroToken.IsOrMap<Token>();
                    token.Tenant = t; // new[] { t }.ToList(); // reset to store only one tenant details

                    return token.CreatTableEntity(nameof(PartitionKeyValue.Xero), t.TenantId.ToString());
                });

            await records.UpdateIn(_table);
        }

        public async Task<XeroOAuth2Token> GetStoredToken(string tenantId = null)
        {
            // strategy 1 by query
            async Task<Token> GetTheFirstOne()
            {
                var query = _table.CreateQuery<TableEntityAdapter<Token>>()
                    .Where(r => r.PartitionKey == nameof(PartitionKeyValue.Xero))
                    .AsTableQuery();

                var resp = await _table.ExecuteQuerySegmentedAsync(query, null);

                return resp.Results.FirstOrDefault()?.OriginalEntity;
            }

            // strategy 2 by read item
            async Task<Token> GetByTenantId()
            {
                var resp = await _table.Retrieve<Token>(nameof(PartitionKeyValue.Xero), tenantId);

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