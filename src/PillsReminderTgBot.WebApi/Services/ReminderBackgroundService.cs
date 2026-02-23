using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PillsReminderTgBot.WebApi.Models;
using PillsReminderTgBot.WebApi.Options;

namespace PillsReminderTgBot.WebApi.Services;

public sealed class ReminderBackgroundService : BackgroundService
{
    private static readonly TimeSpan ConfirmationPollInterval = TimeSpan.FromSeconds(5);

    private readonly IReminderScheduleCalculator _scheduleCalculator;
    private readonly IReminderRecipientStore _recipientStore;
    private readonly IReminderSender _sender;
    private readonly IReminderAcknowledgementStore _ackStore;
    private readonly ITimeZoneResolver _timeZoneResolver;
    private readonly IOptions<ReminderOptions> _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ReminderBackgroundService> _logger;

    public ReminderBackgroundService(
        IReminderScheduleCalculator scheduleCalculator,
        IReminderRecipientStore recipientStore,
        IReminderSender sender,
        IReminderAcknowledgementStore ackStore,
        ITimeZoneResolver timeZoneResolver,
        IOptions<ReminderOptions> options,
        TimeProvider timeProvider,
        ILogger<ReminderBackgroundService> logger)
    {
        _scheduleCalculator = scheduleCalculator;
        _recipientStore = recipientStore;
        _sender = sender;
        _ackStore = ackStore;
        _timeZoneResolver = timeZoneResolver;
        _options = options;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var reminderOptions = _options.Value;
        var timeZone = _timeZoneResolver.Resolve(reminderOptions.TimeZoneId);

        while (!stoppingToken.IsCancellationRequested)
        {
            var nowUtc = _timeProvider.GetUtcNow();
            var nextOccurrenceUtc = _scheduleCalculator.GetNextOccurrence(nowUtc, timeZone, reminderOptions.DailyTime);
            var delay = nextOccurrenceUtc - nowUtc;

            if (delay > TimeSpan.Zero)
            {
                _logger.LogInformation("Следующее напоминание запланировано на {NextOccurrenceUtc:O} (UTC).", nextOccurrenceUtc);
                await _timeProvider.DelayAsync(delay, stoppingToken);
            }

            var recipients = _recipientStore.GetAll();
            if (recipients.Count == 0)
            {
                _logger.LogInformation("Получателей нет. Пропускаю цикл напоминаний.");
                continue;
            }

            var cycleTasks = recipients.Select(chatId => RunReminderCycleAsync(chatId, reminderOptions, stoppingToken)).ToArray();
            await Task.WhenAll(cycleTasks);
        }
    }

    private async Task RunReminderCycleAsync(long chatId, ReminderOptions options, CancellationToken stoppingToken)
    {
        var cycleId = Guid.NewGuid();
        var cycleStartUtc = _timeProvider.GetUtcNow();
        var repeatUntilUtc = cycleStartUtc + options.Repeat.Window;
        var attempt = 1;

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_ackStore.IsConfirmed(cycleId))
            {
                _logger.LogInformation("Цикл напоминаний подтвержден. CycleId={CycleId} ChatId={ChatId}.", cycleId, chatId);
                return;
            }

            var sentAtUtc = _timeProvider.GetUtcNow();
            var reminderEvent = new ReminderEvent(cycleId, attempt, chatId, cycleStartUtc, sentAtUtc);
            await _sender.SendReminderAsync(reminderEvent, stoppingToken);

            if (sentAtUtc + options.Repeat.Interval > repeatUntilUtc)
            {
                _logger.LogInformation("Окно повторов истекло. CycleId={CycleId} ChatId={ChatId}.", cycleId, chatId);
                return;
            }

            await WaitForConfirmationOrDelayAsync(cycleId, options.Repeat.Interval, stoppingToken);
            attempt++;
        }
    }

    private async Task WaitForConfirmationOrDelayAsync(Guid cycleId, TimeSpan delay, CancellationToken stoppingToken)
    {
        var remaining = delay;

        while (remaining > TimeSpan.Zero && !stoppingToken.IsCancellationRequested)
        {
            if (_ackStore.IsConfirmed(cycleId))
            {
                return;
            }

            var step = remaining <= ConfirmationPollInterval ? remaining : ConfirmationPollInterval;
            await _timeProvider.DelayAsync(step, stoppingToken);
            remaining -= step;
        }
    }
}
