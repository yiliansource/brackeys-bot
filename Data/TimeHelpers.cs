using System;
using System.Globalization;

namespace BrackeysBot
{
    public static class TimeHelpers
    {
        public static string ToTimestamp (this DateTime value)
            => value.ToString ("yyyyMMddHHmmssfff");
    
        public static DateTime ToDateTime (this string value)
        {
            DateTime.TryParseExact (value, "yyyyMMddHHmmssfff", null, DateTimeStyles.AdjustToUniversal, out DateTime result);
            return result;
        }
    }
}