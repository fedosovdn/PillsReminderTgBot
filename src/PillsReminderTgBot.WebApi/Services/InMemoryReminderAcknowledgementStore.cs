using System.Collections.Concurrent;

namespace PillsReminderTgBot.WebApi.Services;

public sealed class InMemoryReminderAcknowledgementStore : IReminderAcknowledgementStore
{
    private readonly ConcurrentDictionary<Guid, byte> _confirmed = new();

    public bool IsConfirmed(Guid cycleId) => _confirmed.ContainsKey(cycleId);

    public void Confirm(Guid cycleId) => _confirmed.TryAdd(cycleId, 0);
}
