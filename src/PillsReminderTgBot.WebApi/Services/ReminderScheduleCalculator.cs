namespace PillsReminderTgBot.WebApi.Services;

public sealed class ReminderScheduleCalculator : IReminderScheduleCalculator
{
    public DateTimeOffset GetNextOccurrence(DateTimeOffset nowUtc, TimeZoneInfo timeZone, TimeSpan dailyTime)
    {
        var nowLocal = TimeZoneInfo.ConvertTime(nowUtc, timeZone);
        var todayLocalMidnight = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, 0, 0, 0, DateTimeKind.Unspecified);
        var scheduledLocal = todayLocalMidnight.Add(dailyTime);

        if (nowLocal.TimeOfDay > dailyTime)
        {
            scheduledLocal = scheduledLocal.AddDays(1);
        }

        var scheduledUtc = TimeZoneInfo.ConvertTimeToUtc(scheduledLocal, timeZone);
        return new DateTimeOffset(scheduledUtc, TimeSpan.Zero);
    }
}
