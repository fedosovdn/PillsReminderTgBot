using Microsoft.Extensions.Options;
using PillsReminderTgBot.WebApi.Options;

namespace PillsReminderTgBot.WebApi.Services;

public sealed class ReminderStatusService : IReminderStatusService
{
    private readonly IReminderRecipientStore _recipientStore;
    private readonly IReminderScheduleCalculator _scheduleCalculator;
    private readonly ITimeZoneResolver _timeZoneResolver;
    private readonly IOptions<ReminderOptions> _options;
    private readonly TimeProvider _timeProvider;

    public ReminderStatusService(
        IReminderRecipientStore recipientStore,
        IReminderScheduleCalculator scheduleCalculator,
        ITimeZoneResolver timeZoneResolver,
        IOptions<ReminderOptions> options,
        TimeProvider timeProvider)
    {
        _recipientStore = recipientStore;
        _scheduleCalculator = scheduleCalculator;
        _timeZoneResolver = timeZoneResolver;
        _options = options;
        _timeProvider = timeProvider;
    }

    public string GetStatus(long chatId)
    {
        if (!_recipientStore.Contains(chatId))
        {
            return "Напоминания сейчас отключены. Включить — /start.";
        }

        var reminderOptions = _options.Value;
        var timeZone = _timeZoneResolver.Resolve(reminderOptions.TimeZoneId);
        var nextUtc = _scheduleCalculator.GetNextOccurrence(_timeProvider.GetUtcNow(), timeZone, reminderOptions.DailyTime);
        var nextLocal = TimeZoneInfo.ConvertTime(nextUtc, timeZone);

        return $"Напоминания включены. Следующее — {nextLocal:dd.MM.yyyy HH:mm} ({timeZone.StandardName}).";
    }
}
