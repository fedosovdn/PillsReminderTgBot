namespace PillsReminderTgBot.WebApi.Models;

public sealed record ReminderEvent(
    Guid CycleId,
    int Attempt,
    long ChatId,
    DateTimeOffset ScheduledAtUtc,
    DateTimeOffset SentAtUtc);
