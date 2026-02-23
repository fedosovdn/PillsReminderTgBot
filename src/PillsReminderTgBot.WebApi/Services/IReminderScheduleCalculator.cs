namespace PillsReminderTgBot.WebApi.Services;

public interface IReminderScheduleCalculator
{
    DateTimeOffset GetNextOccurrence(DateTimeOffset nowUtc, TimeZoneInfo timeZone, TimeSpan dailyTime);
}
