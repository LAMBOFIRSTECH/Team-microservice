namespace Teams.INFRA.Layer.Helperss;
public static class DateHelper
{
    public static DateTimeOffset ParseToLocal(this string timeZoneId, DateTimeOffset dateTimeOffset)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var local = TimeZoneInfo.ConvertTime(dateTimeOffset, tz);
        return local;
    }
}