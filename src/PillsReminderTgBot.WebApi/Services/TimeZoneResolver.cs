using Microsoft.Extensions.Logging;

namespace PillsReminderTgBot.WebApi.Services;

public sealed class TimeZoneResolver : ITimeZoneResolver
{
    private static readonly IReadOnlyDictionary<string, string> WindowsToIana = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Russian Standard Time"] = "Europe/Moscow"
    };

    private readonly ILogger<TimeZoneResolver> _logger;

    public TimeZoneResolver(ILogger<TimeZoneResolver> logger)
    {
        _logger = logger;
    }

    public TimeZoneInfo Resolve(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            _logger.LogWarning("TimeZoneId пустой. Переходим на UTC.");
            return TimeZoneInfo.Utc;
        }

        if (TryFind(timeZoneId, out var timeZone))
        {
            return timeZone;
        }

        if (WindowsToIana.TryGetValue(timeZoneId, out var ianaId) && TryFind(ianaId, out timeZone))
        {
            _logger.LogInformation("Сопоставлен идентификатор часового пояса {WindowsId} с {IanaId}.", timeZoneId, ianaId);
            return timeZone;
        }

        _logger.LogWarning("Неизвестный идентификатор часового пояса {TimeZoneId}. Переходим на UTC.", timeZoneId);
        return TimeZoneInfo.Utc;
    }

    private static bool TryFind(string timeZoneId, out TimeZoneInfo timeZone)
    {
        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            timeZone = TimeZoneInfo.Utc;
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            timeZone = TimeZoneInfo.Utc;
            return false;
        }
    }
}
