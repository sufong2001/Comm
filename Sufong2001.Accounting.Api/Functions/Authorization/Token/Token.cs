using System;
using Newtonsoft.Json;
using Sufong2001.Accounting.Api.Storage;
using Xero.NetStandard.OAuth2.Models;

namespace Sufong2001.Accounting.Api.Functions.Authorization.Token
{
    public class Token : IStorageEnitity
    {
        public string Id { get; set; }

        public string Pk { get; set; }

        public Tenant Tenant { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string IdToken { get; set; }

        public DateTime ExpiresAtUtc { get; set; }
    }
}