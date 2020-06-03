using AutoMapper;
using Microsoft.Azure.Cosmos.Table;
using Sufong2001.Accounting.Api.Storage.Names;
using Sufong2001.Accounting.Api.Storage.Token;
using Sufong2001.Core.Storage.Interfaces;
using Sufong2001.Share.AzureStorage;
using Sufong2001.Share.Json;
using System;
using System.Linq;
using Xero.NetStandard.OAuth2.Token;

namespace Sufong2001.Accounting.Api.Storage
{
    public interface ITokenTable
    {
        void StoreToken(XeroOAuth2Token xeroToken);

        //Task<XeroOAuth2Token> GetStoredToken(string tenantId = null);

        XeroOAuth2Token GetStoredToken(string tenantId = null);

        bool TokenExists();

        void DestroyToken();
    }

    public class TokenTable : ITokenTable
    {
        private readonly CloudTable _table;
        private IMapper _mapper;

        public TokenTable(ITableRepository storgeRepository, IMapper mapper)
        {
            _table = storgeRepository.GetTable(nameof(TableName.AccoTokens));
            _mapper = mapper;
        }

        public async void StoreToken(XeroOAuth2Token xeroToken)
        {
            var records = xeroToken.Tenants
                .Select(t =>
                {
                    var token = xeroToken.IsOrMap<TokenRow>();
                    token.Tenant = t; // new[] { t }.ToList(); // reset to store only one tenant details

                    return token.CreatTableEntity(nameof(TablePartition.Xero), t.TenantId.ToString());
                });

            await records.UpdateIn(_table);
        }

        public XeroOAuth2Token GetStoredToken(string tenantId = null)
        {
            var query = _table.CreateQuery<TableEntityAdapter<TokenRow>>()
                .Where(x => x.PartitionKey == nameof(TablePartition.Xero));

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