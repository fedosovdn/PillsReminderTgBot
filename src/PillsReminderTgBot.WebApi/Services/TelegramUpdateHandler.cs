using Microsoft.Extensions.Logging;
using PillsReminderTgBot.WebApi.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PillsReminderTgBot.WebApi.Services;

public sealed class TelegramUpdateHandler : ITelegramUpdateHandler
{
    private const string AckPrefix = "ack:";
    private readonly IReminderRecipientStore _recipientStore;
    private readonly IReminderAcknowledgementStore _ackStore;
    private readonly IReminderStatusService _statusService;
    private readonly IReminderSender _sender;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<TelegramUpdateHandler> _logger;

    public TelegramUpdateHandler(
        IReminderRecipientStore recipientStore,
        IReminderAcknowledgementStore ackStore,
        IReminderStatusService statusService,
        IReminderSender sender,
        TimeProvider timeProvider,
        ILogger<TelegramUpdateHandler> logger)
    {
        _recipientStore = recipientStore;
        _ackStore = ackStore;
        _statusService = statusService;
        _sender = sender;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Telegram polling error.");
        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message is not null)
        {
            await HandleMessageAsync(botClient, update.Message, cancellationToken);
            return;
        }

        if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery is not null)
        {
            await HandleCallbackAsync(botClient, update.CallbackQuery, cancellationToken);
        }
    }

    private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (message.Chat.Type != ChatType.Private || string.IsNullOrWhiteSpace(message.Text))
        {
            return;
        }

        var command = message.Text.Trim();
        if (string.Equals(command, "/start", StringComparison.OrdinalIgnoreCase))
        {
            _recipientStore.Register(message.Chat.Id);
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "Готово! Я буду напоминать вам каждый день в 07:00 (MSK).",
                cancellationToken: cancellationToken);
            return;
        }

        if (string.Equals(command, "/help", StringComparison.OrdinalIgnoreCase))
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "Команды:\n/start — включить напоминания\n/stop — отключить\n/status — текущий статус\n/test — тестовое напоминание",
                cancellationToken: cancellationToken);
            return;
        }

        if (string.Equals(command, "/status", StringComparison.OrdinalIgnoreCase))
        {
            var status = _statusService.GetStatus(message.Chat.Id);
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: status,
                cancellationToken: cancellationToken);
            return;
        }

        if (string.Equals(command, "/test", StringComparison.OrdinalIgnoreCase))
        {
            var nowUtc = _timeProvider.GetUtcNow();
            var reminderEvent = new ReminderEvent(
                CycleId: Guid.NewGuid(),
                Attempt: 1,
                ChatId: message.Chat.Id,
                ScheduledAtUtc: nowUtc,
                SentAtUtc: nowUtc);

            await _sender.SendReminderAsync(reminderEvent, cancellationToken);
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "Тестовое напоминание отправлено.",
                cancellationToken: cancellationToken);
            return;
        }

        if (string.Equals(command, "/stop", StringComparison.OrdinalIgnoreCase))
        {
            _recipientStore.Unregister(message.Chat.Id);
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "Ок, напоминания отключены. Чтобы снова включить — /start.",
                cancellationToken: cancellationToken);
        }
    }

    private async Task HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var data = callbackQuery.Data;
        if (string.IsNullOrWhiteSpace(data) || !data.StartsWith(AckPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var idPart = data[AckPrefix.Length..];
        if (!Guid.TryParse(idPart, out var cycleId))
        {
            _logger.LogWarning("Invalid callback data: {CallbackData}", data);
            return;
        }

        _ackStore.Confirm(cycleId);

        if (callbackQuery.Message is not null)
        {
            await botClient.AnswerCallbackQuery(
                callbackQueryId: callbackQuery.Id,
                text: "Отлично, отметил!",
                cancellationToken: cancellationToken);
        }
    }
}
