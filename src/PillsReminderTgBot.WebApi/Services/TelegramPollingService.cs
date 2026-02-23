using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace PillsReminderTgBot.WebApi.Services;

public sealed class TelegramPollingService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ITelegramUpdateHandler _updateHandler;
    private readonly ILogger<TelegramPollingService> _logger;

    public TelegramPollingService(
        ITelegramBotClient botClient,
        ITelegramUpdateHandler updateHandler,
        ILogger<TelegramPollingService> logger)
    {
        _botClient = botClient;
        _updateHandler = updateHandler;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Запуск опроса Telegram.");
        var offset = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var updates = await _botClient.GetUpdates(
                    offset: offset,
                    timeout: 30,
                    allowedUpdates: new[] { UpdateType.Message, UpdateType.CallbackQuery },
                    cancellationToken: stoppingToken);

                foreach (var update in updates)
                {
                    offset = update.Id + 1;
                    await _updateHandler.HandleUpdateAsync(_botClient, update, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                await _updateHandler.HandleErrorAsync(ex, stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }
}
