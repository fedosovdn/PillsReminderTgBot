using Microsoft.Extensions.Logging;
using PillsReminderTgBot.WebApi.Models;

namespace PillsReminderTgBot.WebApi.Services;

public sealed class LoggingReminderSender : IReminderSender
{
    private readonly ILogger<LoggingReminderSender> _logger;

    public LoggingReminderSender(ILogger<LoggingReminderSender> logger)
    {
        _logger = logger;
    }

    public Task SendReminderAsync(ReminderEvent reminderEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Reminder sent. CycleId={CycleId} Attempt={Attempt} ChatId={ChatId} ScheduledAtUtc={ScheduledAtUtc:O} SentAtUtc={SentAtUtc:O}",
            reminderEvent.CycleId,
            reminderEvent.Attempt,
            reminderEvent.ChatId,
            reminderEvent.ScheduledAtUtc,
            reminderEvent.SentAtUtc);

        return Task.CompletedTask;
    }
}
