using Microsoft.Extensions.Logging;
using PillsReminderTgBot.WebApi.Models;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PillsReminderTgBot.WebApi.Services;

public sealed class TelegramReminderSender : IReminderSender
{
    private const string AckPrefix = "ack:";
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramReminderSender> _logger;

    public TelegramReminderSender(ITelegramBotClient botClient, ILogger<TelegramReminderSender> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task SendReminderAsync(ReminderEvent reminderEvent, CancellationToken cancellationToken)
    {
        var keyboard = new InlineKeyboardMarkup(
            InlineKeyboardButton.WithCallbackData("Я выпил", $"{AckPrefix}{reminderEvent.CycleId}"));

        await _botClient.SendMessage(
            chatId: reminderEvent.ChatId,
            text: "Напоминание: пора принять таблетку.",
            parseMode: ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Сообщение-напоминание отправлено. CycleId={CycleId} Attempt={Attempt} ChatId={ChatId}.",
            reminderEvent.CycleId,
            reminderEvent.Attempt,
            reminderEvent.ChatId);
    }
}
