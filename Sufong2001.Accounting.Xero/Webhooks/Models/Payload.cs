using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sufong2001.Accounting.Xero.Webhooks.Models
{
    public class Payload
    {
        [JsonProperty("events")]
        public List<Event> Events { get; set; }

        [JsonProperty("firstEventSequence")]
        public int FirstEventSequence { get; set; }

        [JsonProperty("lastEventSequence")]
        public int LastEventSequence { get; set; }

        [JsonProperty("entropy")]
        public string Entropy { get; set; }

    }
}