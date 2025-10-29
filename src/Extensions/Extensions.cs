namespace PlotThoseLines.Extensions
{
    public static class Extensions
    {
        public static DateTime ToLocalDateTimeFromUnixMs(this long unixMs) 
            => DateTimeOffset.FromUnixTimeMilliseconds(unixMs).LocalDateTime;

        public static string ToShortNumber(this decimal number)
        {
            if (number >= 1_000_000_000)
                return $"{number / 1_000_000_000:0.##}B";
            if (number >= 1_000_000)
                return $"{number / 1_000_000:0.##}M";
            if (number >= 1_000)
                return $"{number / 1_000:0.##}K";
            return number.ToString("0.##");
        }

        public static string ToShortNumber(this double number)
            => ToShortNumber((decimal)number);
    }
}
