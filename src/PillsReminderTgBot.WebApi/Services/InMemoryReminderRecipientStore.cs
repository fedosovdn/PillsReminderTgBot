using System.Collections.Concurrent;

namespace PillsReminderTgBot.WebApi.Services;

public sealed class InMemoryReminderRecipientStore : IReminderRecipientStore
{
    private readonly ConcurrentDictionary<long, byte> _recipients = new();

    public IReadOnlyCollection<long> GetAll() => _recipients.Keys.ToArray();

    public void Register(long chatId) => _recipients.TryAdd(chatId, 0);

    public bool Unregister(long chatId) => _recipients.TryRemove(chatId, out _);

    public bool Contains(long chatId) => _recipients.ContainsKey(chatId);
}
