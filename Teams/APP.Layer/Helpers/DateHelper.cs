namespace Teams.APP.Layer.Helpers;
public static class DateHelper
{
    public static DateTimeOffset ParseToLocal(this string timeZoneId, DateTimeOffset dateTimeOffset)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var local = TimeZoneInfo.ConvertTime(dateTimeOffset, tz);
        return local;
    }
}