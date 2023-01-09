namespace System;

public static class DateTimeExtensions
{
    public static DateTime SameOrNext(this DateTime start, DayOfWeek day)
    {
        return start.DayOfWeek == day ? start : start.Next(day);
    }

    public static DateTime Next(this DateTime start, DayOfWeek day)
    {
        do
        {
            start = start.AddDays(1);
        } while (start.DayOfWeek != day);

        return start;
    }

    public static DateTime SameOrPrevious(this DateTime start, DayOfWeek day)
    {
        return start.DayOfWeek == day ? start : start.Previous(day);
    }

    public static DateTime Previous(this DateTime start, DayOfWeek day)
    {
        do
        {
            start = start.AddDays(-1);
        } while (start.DayOfWeek != day);

        return start;
    }


    public static DateTime EndOfDay(this DateTime date)
    {
        return new(date.Year, date.Month, date.Day, 23, 59, 59, 999, date.Kind);
    }


    public static DateTime StartOfDay(this DateTime date)
    {
        return date.Date;
    }
}
