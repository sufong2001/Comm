using System;
using Newtonsoft.Json;
using Xero.NetStandard.OAuth2.Models;

namespace Sufong2001.Accounting.Api.Functions.Authorization.Token
{
    public class Token
    {
        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "pk", NullValueHandling = NullValueHandling.Ignore)]
        public string Pk { get; set; }

        public Tenant Tenant { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string IdToken { get; set; }

        public DateTime ExpiresAtUtc { get; set; }
    }
}