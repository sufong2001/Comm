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

        /// <summary>
        /// Clone the object utilising Newtonsoft.Json JObject
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T JClone<T>(this object obj)
        {
            return obj != null ? obj.ToJObject().ToObject<T>() : default(T);
        }


        /// <summary>
        /// If the pass-in object is same as the target Type, than it will be cast and return the same object as the target type.
        /// Otherwise a new object will be created and copy all the mapping property values to the new object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T IsOrMap<T>(this object obj)
        {
            if (obj is T o) return o;


            return obj != null ? obj.ToJObject().ToObject<T>() : default(T);
        }

        /// <summary>
        /// Merge the objects utilising Newtonsoft.Json JObject
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objs"></param>
        /// <param name="mergeSettings"></param>
        /// <returns></returns>
        public static T MergeTo<T>(this object[] objs, JsonMergeSettings mergeSettings = null)
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