using Newtonsoft.Json;

namespace Sufong2001.Accounting.Api.Storage
{
    public interface IStorageEnitity
    {
        [JsonProperty("id")]
        string Id { get; set; }


        [JsonProperty("pk")]
        string Pk { get; set; }
    }
}