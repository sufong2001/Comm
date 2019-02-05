using System;
using DateTime = System.DateTime;

namespace Sufong2001.Share.Date
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Return a GUID with yyyyMMdd prefix.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string DateGuid(this DateTime dateTime, string separator)
        {
            return dateTime.ToUniversalTime().ToString("yyyyMMdd") + separator + Guid.NewGuid();
        }
    }
}