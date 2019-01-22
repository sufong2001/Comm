using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Sufong2001.Share.Json
{
    public static class JsonExtensions
    {
        public static string ToJson(this object obj, Formatting format = Formatting.None)
        {
            return obj == null ? null : JsonConvert.SerializeObject(obj, format);
        }

        public static T To<T>(this string json)
        {
            return json != null ? JsonConvert.DeserializeObject<T>(json) : default(T);
        }

        public static JObject ToJObject(this object obj)
        {
            return obj != null ? JObject.FromObject(obj) : default(JObject);
        }

        /// <summary>
        /// Clone the object utilising Newtonsoft.Json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T JsonClone<T>(this object obj)
        {
            return obj != null ? obj.ToJson().To<T>() : default(T);
        }

        public static T MapJsonTo<T>(this object[] objs, JsonMergeSettings mergeSettings = null)
        {
            if (objs == null || !objs.Any()) return default(T);

            mergeSettings = mergeSettings ?? new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union,
            };

            var val = objs.Select(ToJObject).Aggregate((v, o) =>
            {
                v.Merge(o, mergeSettings);

                return v;
            });

            return val.ToObject<T>();
        }
    }
}