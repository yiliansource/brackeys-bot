using System;

namespace BrackeysBot.Data
{
    public static class TimeHelpers
    {
        public static string ToTimestamp (this DateTime value)
            => value.ToString ("yyyyMMddHHmmssfff");
    
        public static DateTime ToDateTime (this string value)
            => DateTime.Parse (value);
    }
}