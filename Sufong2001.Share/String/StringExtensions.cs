using System;
using System.Collections.Generic;
using System.Linq;

namespace Sufong2001.Share.String
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNotNullOrEmpty(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        public static string Truncate(this string valueToTruncate, int startIndex = 0, int maxLength = 0
            , TruncateOptions options = TruncateOptions.FinishWord)
        {
            if (valueToTruncate == null)
            {
                return "";
            }

            if (startIndex > 0 && startIndex < valueToTruncate.Length)
            {
                valueToTruncate = valueToTruncate.Substring(startIndex);
            }

            if (valueToTruncate.Length <= maxLength)
            {
                return valueToTruncate;
            }

            bool includeEllipsis = (options & TruncateOptions.IncludeEllipsis) ==
                                   TruncateOptions.IncludeEllipsis;
            bool finishWord = (options & TruncateOptions.FinishWord) ==
                              TruncateOptions.FinishWord;
            bool allowLastWordOverflow =
                (options & TruncateOptions.AllowLastWordToGoOverMaxLength) ==
                TruncateOptions.AllowLastWordToGoOverMaxLength;

            string retValue = valueToTruncate;

            if (includeEllipsis)
            {
                maxLength -= 1;
            }

            int lastSpaceIndex = retValue.LastIndexOf(" ",
                maxLength, StringComparison.CurrentCultureIgnoreCase);

            if (!finishWord)
            {
                retValue = retValue.Remove(maxLength);
            }
            else if (allowLastWordOverflow)
            {
                int spaceIndex = retValue.IndexOf(" ",
                    maxLength, StringComparison.CurrentCultureIgnoreCase);
                if (spaceIndex != -1)
                {
                    retValue = retValue.Remove(spaceIndex);
                }
            }
            else if (lastSpaceIndex > -1)
            {
                retValue = retValue.Remove(lastSpaceIndex);
            }

            if (includeEllipsis && retValue.Length < valueToTruncate.Length)
            {
                retValue += "&hellip;";
            }
            return retValue;
        }

        public static string TruncateAt(this string valueToTruncate, params char[] chars)
        {
            if (chars == null) return valueToTruncate;

            string retValue = valueToTruncate;

            foreach (char c in chars)
            {
                int index = retValue.IndexOf(c);
                if (index > 0)
                {
                    retValue = retValue.Remove(index);
                }
            }

            return retValue;
        }

        public static string TruncateAfterTheLast(this string valueToTruncate, params char[] chars)
        {
            if (chars == null) return valueToTruncate;

            string retValue = valueToTruncate;

            foreach (char c in chars)
            {
                int index = retValue.LastIndexOf(c);
                if (index > 0 && index + 1 < retValue.Length)
                {
                    retValue = retValue.Remove(index + 1);
                }
            }

            return retValue;
        }

        public static TOutput[] Split<TOutput>(this string val, char[] separators = null,
            StringSplitOptions options = StringSplitOptions.None)
        {
            return val.Split<TOutput>(null, separators, options);
        }

        public static TOutput[] Split<TOutput>(this string val, TOutput[] defReturn, char[] separators = null,
            StringSplitOptions options = StringSplitOptions.None)
        {
            if (string.IsNullOrEmpty(val)) return defReturn;

            if (separators == null || separators.Length < 1) separators = new char[] { ',' };

            string[] retVal = val.Split(separators, options);

            return retVal.Select(s => s.ConvertTo<TOutput>()).ToArray();
        }

        public static int[] SplitWithRange(this string val, char[] separators = null,
            StringSplitOptions options = StringSplitOptions.None)
        {
            return val.Split(new string[0])
                .Aggregate(new List<int>(), (l, i) =>
                {
                    var vals = i.Split<int>(new int[0], separators, options);

                    if (vals.Length > 1)
                    {
                        for (var x = vals.Min() + 1; x < vals.Max(); x++)
                        {
                            l.Add(x);
                        }
                    }

                    l.AddRange(vals);

                    return l;
                })
                .OrderBy(i => i).ToArray();
        }

        public static string ArrayToString(this Array array, string separator = ",", string itemFormat = "{0}")
        {
            if (array == null) return null;

            string[] retVal = array.Cast<object>()
                .Where(s => s != null && !string.IsNullOrEmpty(s.ToString()))
                .Select(s => string.Format(itemFormat, s)).ToArray();

            return string.Join(separator, retVal);
        }

        /// <summary>
        /// Convert a string to a specified type or the provided default value if it IsNullOrEmpty.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="val"></param>
        /// <param name="dVal">default value</param>
        /// <returns></returns>
        public static TOutput ConvertTo<TOutput>(this string val, TOutput dVal = default(TOutput))
        {
            if (string.IsNullOrEmpty(val)) return dVal;

            Type t = typeof(TOutput);
            Type u = Nullable.GetUnderlyingType(t);
            t = u ?? t;

            if (t.IsEnum) return (TOutput)Enum.Parse(t, val);

            object obj = val;

            if (t == typeof(DateTime))
            {
                DateTime parseDateTime;
                var success = DateTime.TryParse(val, out parseDateTime);
                obj = parseDateTime;

                return success ? (TOutput)obj : dVal;
            }

            if (t != typeof(bool))
                return (TOutput)Convert.ChangeType(obj, t);

            // additional Boolean conversion checking
            switch (val.ToLower())
            {
                case "true":
                case "false":
                    break;

                case "1":
                case "on":
                    obj = bool.TrueString;
                    break;

                default:
                    return dVal;
            }

            return (TOutput)Convert.ChangeType(obj, t);
        }

        public static bool ConvertToBoolen(this string val)
        {
            if (string.IsNullOrEmpty(val)) return false;

            Type t = typeof(bool);

            var obj = val;

            // additional Boolean conversion checking
            switch (val.ToLower())
            {
                case "true":
                case "false":
                    break;

                case "yes":
                case "y":
                case "1":
                case "on":
                    obj = bool.TrueString;
                    break;

                default:
                    return false;
            }

            return (bool)Convert.ChangeType(obj, t);
        }

        public static int ConvertToBit(this string val)
        {
            var rt = 0;

            if (string.IsNullOrEmpty(val)) return rt;

            switch (val.ToLower())
            {
                case "true":
                case "yes":
                case "y":
                case "1":
                case "on":
                    rt = 1;
                    break;
            }

            return rt;
        }

    }
}
