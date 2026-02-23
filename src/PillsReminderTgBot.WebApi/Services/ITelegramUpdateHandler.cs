using Telegram.Bot;
using Telegram.Bot.Types;

namespace PillsReminderTgBot.WebApi.Services;

public interface ITelegramUpdateHandler
{
    Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken);
}
