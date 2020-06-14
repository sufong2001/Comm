using Newtonsoft.Json;
using System;

namespace Sufong2001.Accounting.Xero.Webhooks.Models
{
    public class Event
    {
        [JsonProperty("resourceUrl")]
        public string ResourceUrl { get; set; }

        [JsonProperty("resourceId")]
        public Guid ResourceId { get; set; }

        [JsonProperty("eventDateUtc")]
        public DateTime EventDateUtc { get; set; }

        [JsonProperty("eventType")]
        public string EventType { get; set; }

        [JsonProperty("eventCategory")]
        public string EventCategory { get; set; }

        [JsonProperty("tenantId")]
        public Guid TenantId { get; set; }

        [JsonProperty("tenantType")]
        public string TenantType { get; set; }
    }
}