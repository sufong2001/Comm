using Microsoft.Azure.Cosmos.Table;
using Sufong2001.Core.Storage.Interfaces;
using Sufong2001.Share.AzureStorage;
using Sufong2001.Share.Json;
using System;
using System.Linq;
using Sufong2001.Accounting.Api.Storage.Token.Names;
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

        public async void StoreToken(XeroOAuth2Token xeroToken)
        {
            var records = xeroToken.Tenants
                .Select(t =>
                {
                    var token = xeroToken.IsOrMap<Token>();
                    token.Tenant = t; // new[] { t }.ToList(); // reset to store only one tenant details

                    return token.CreatTableEntity(nameof(PartitionValue.Xero), t.TenantId.ToString());
                });

            await records.UpdateIn(_table);
        }

        public XeroOAuth2Token GetStoredToken(string tenantId = null)
        {
            var query = _table.CreateQuery<TableEntityAdapter<Token>>()
                .Where(x => x.PartitionKey == nameof(PartitionValue.Xero));

            if (tenantId != null)
            {
                query = query.Where(x => x.RowKey == tenantId);
            }

            var result = query.FirstOrDefault()?.OriginalEntity;

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