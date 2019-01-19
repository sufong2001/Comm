using Newtonsoft.Json;

namespace Sufong2001.Share.Json
{
    public static class JsonExtensions
    {
        public static string ToJson(this object obj, Formatting format = Formatting.None)
        {
            if (obj == null) return null;

            return JsonConvert.SerializeObject(obj, format);
        }
    }
}