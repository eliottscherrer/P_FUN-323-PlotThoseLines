namespace PlotThoseLines.Extensions
{
    public static class Extensions
    {
        public static DateTime ToLocalDateTimeFromUnixMs(this long unixMs) 
            => DateTimeOffset.FromUnixTimeMilliseconds(unixMs).LocalDateTime;
    }
}
