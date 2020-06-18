using Xero.NetStandard.OAuth2.Model.Accounting;

namespace Sufong2001.Accounting.Api.Storage.Model
{
    public class InvoiceEntity : Invoice, IStorageEnitity
    {
        public string Id { get; set; }

        public string Pk { get; set; }
    }
}