using System;
using Xero.NetStandard.OAuth2.Models;

namespace Sufong2001.Accounting.Api.Storage.Token
{
    public class TokenRow
    {
        public Tenant Tenant { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string IdToken { get; set; }

        public DateTime ExpiresAtUtc { get; set; }
    }
}