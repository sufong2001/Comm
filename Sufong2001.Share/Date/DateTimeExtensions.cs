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
        /// <returns></returns>
        public static string DateGuid(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("yyyyMMdd") + "-" + Guid.NewGuid();
        }
    }
}