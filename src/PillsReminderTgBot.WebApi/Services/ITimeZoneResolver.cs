namespace PillsReminderTgBot.WebApi.Services;

public interface ITimeZoneResolver
{
    TimeZoneInfo Resolve(string timeZoneId);
}
