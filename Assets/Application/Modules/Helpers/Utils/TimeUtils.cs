using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class TimeUtils
{
    public static float GetMillsFromMins(float minutes)
    {
        return minutes * 60f * 1000f;
    }
    
    public static float GetMillsFromSecs(float seconds)
    {
        return seconds * 1000f;
    }
    
    public static float GetSecsFromMillis(float milliseconds)
    {
        return milliseconds / 1000f;
    }
    
    public static float GetSecsFromMins(float minutes)
    {
        return minutes * 60f;
    }

    public static float GetSecsFromHours(float hours)
    {
        return hours * 60f * 60f;
    }

    public static float GetMinsFromMills(float milliseconds)
    {
        return milliseconds / 60f / 1000f;
    }

    public static float GetSecondsToMidnight()
    {
        var current = DateTime.Now;
        var tomorrow = current.AddDays(1).Date;

        return (float)(tomorrow - current).TotalSeconds;
    }
    public static float GetSecondsBetween(DateTime current, DateTime target)
    {
        return (float)(target - current).TotalSeconds;
    }
    public static string GetCurrentDateString()
    {
        return DateTime.Today.Date.ToShortDateString();
    }
    
    public static int CurrentDayHashCode()
    {
        return DateTime.Today.ToString().GetHashCode();
    }

    public static int CurrentWeekHashCode()
    {
        var currentDate = DateTime.Today;
        var weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(currentDate, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        var weekString = $"{currentDate.Year}-W{weekNumber}";
        
        return weekString.GetHashCode();
    }
    
    public static int CurrentMonthHashCode()
    {
        var currentDate = DateTime.Today;
        var monthString = $"{currentDate.Year}-M{currentDate.Month}";
        
        return monthString.GetHashCode();
    }

    public static float GetMillsPassed(long closerTimestamp, long fartherTimestamp)
    {
        return closerTimestamp - fartherTimestamp;
    }

    private static (DateTime epochStart, int offset) GetEpochStartAndOffset()
    {
        var offset = (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMinutes;
        var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (epochStart, offset);
    }

    public static string FormatTime(float seconds)
    {
        var timeSpan = TimeSpan.FromSeconds(seconds);
        if (timeSpan.TotalDays >= 1)
        {
            return FormatTimeWithDays(timeSpan);
        } 
        return timeSpan.Hours > 0 ? $"{timeSpan.Hours:D2}h:{timeSpan.Minutes:D2}m:{timeSpan.Seconds:D2}s" : $"{timeSpan.Minutes:D2}m:{timeSpan.Seconds:D2}s";
    }
    
    public static string FormatTimeWithoutText(float seconds)
    {
        var timeSpan = TimeSpan.FromSeconds(seconds);
        if (timeSpan.TotalDays >= 1)
        {
            return FormatTimeWithDays(timeSpan);
        } 
        return timeSpan.Hours > 0 ? $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}" : $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }
    private static string FormatTimeWithDays(TimeSpan timeSpan)
    {
        int days = (int)timeSpan.TotalDays;
        int hours = timeSpan.Hours;
        int minutes = timeSpan.Minutes;
        int seconds = timeSpan.Seconds;

        if (hours >= 24)
        {
            days += hours / 24;
            hours %= 24;
        }

        return $"{days:00}d:{hours:00}h:{minutes:00}m:{seconds:00}s";
    }

    public static bool IsAnotherDay(long now, long before)
    {
        var epoch = GetEpochStartAndOffset();
        
        var nowDay = epoch.epochStart.AddMilliseconds(now).AddMinutes(epoch.offset);
        var beforeDay = epoch.epochStart.AddMilliseconds(before).AddMinutes(epoch.offset);
        return nowDay.Date > beforeDay.Date;
    }

    public static bool IsAnotherWeek(long now, long before)
    {
        var nowDay = DateTimeOffset.FromUnixTimeMilliseconds(now).UtcDateTime;
        var beforeDay = DateTimeOffset.FromUnixTimeMilliseconds(before).UtcDateTime;
    
        int currentWeekNumber = GetWeekOfYearCalendar(nowDay);
        int referenceWeekNumber = GetWeekOfYearCalendar(beforeDay);
        
        int currentYear = nowDay.Year;
        int referenceYear = beforeDay.Year;

        return currentYear > referenceYear || 
               (currentYear == referenceYear && currentWeekNumber > referenceWeekNumber);
    }

    public static bool IsAnotherMonth(long now, long before)
    {
        var epoch = GetEpochStartAndOffset();

        var nowDay = epoch.epochStart.AddMilliseconds(now).AddMinutes(epoch.offset);
        var beforeDay = epoch.epochStart.AddMilliseconds(before).AddMinutes(epoch.offset);

        return nowDay.Month != beforeDay.Month || nowDay.Year != beforeDay.Year;
    }

    private static int GetWeekOfYear(DateTime time)
    {
        DateTime jan1 = new DateTime(time.Year, 1, 1);
        int daysOffset = (7 - (int)jan1.DayOfWeek) % 7;
        DateTime firstSunday = jan1.AddDays(daysOffset);

        int weekNumber = (time.DayOfYear - firstSunday.DayOfYear) / 7 + 1;

        if (weekNumber <= 0)
        {
            weekNumber = GetWeekOfYear(jan1.AddDays(-1));
        }
        else if (weekNumber == 53 && firstSunday.AddDays(7 * (weekNumber - 1)).Year != time.Year)
        {
            weekNumber = 1;
        }

        return weekNumber;
    }
    private static int GetWeekOfYearCalendar(DateTime date)
    {
        var culture = CultureInfo.CurrentCulture;
        var calendar = culture.Calendar;
        var firstDayOfWeek = DayOfWeek.Monday;
    
        return calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);

    }

    public static float GetSecondsToMidnightSpecificDayOfTheWeek(int dayIndex)
    {
        var targetDay = (DayOfWeek)dayIndex;
        var currentTime = DateTime.Now;
        var targetMidnight = currentTime.AddDays(1).Date;

        while (targetMidnight.DayOfWeek != targetDay)
        {
            targetMidnight = targetMidnight.AddDays(1).Date;
        }

        var timeDifference = targetMidnight - currentTime;

        return (float)timeDifference.TotalSeconds;
    }

    public static float GetSecondsToMidnightLastDayOfTheMonth()
    {
        var currentTime = DateTime.Now;

        var lastDayOfMonth = LastDayOfMonth(currentTime).AddDays(1).Date;
        var timeDifference = lastDayOfMonth - currentTime;
        return (float)timeDifference.TotalSeconds;
    }

    public static DateTime FirstDayOfMonth(DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, 1);
    }

    public static DateTime LastDayOfMonth(DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month));
    }

    public static int GetDayInMonth(long timestamp)
    {
        var date = GetCurrentDate(timestamp);
        return date.Day;
    }
    
    public static int GetDaysInMonth(long timestamp)
    {
        var date = GetCurrentDate(timestamp);
        return DateTime.DaysInMonth(date.Year, date.Month);
    }
    
    public static DateTime GetCurrentDate(long timestamp)
    {
        var epoch = GetEpochStartAndOffset();

        return epoch.epochStart.AddMilliseconds(timestamp).AddMinutes(epoch.offset);
    }
}