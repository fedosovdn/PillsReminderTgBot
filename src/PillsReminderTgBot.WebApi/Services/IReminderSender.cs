using PillsReminderTgBot.WebApi.Models;

namespace PillsReminderTgBot.WebApi.Services;

public interface IReminderSender
{
    Task SendReminderAsync(ReminderEvent reminderEvent, CancellationToken cancellationToken);
}
