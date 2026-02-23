namespace PillsReminderTgBot.WebApi.Services;

public interface IReminderRecipientStore
{
    IReadOnlyCollection<long> GetAll();
    void Register(long chatId);
    bool Unregister(long chatId);
    bool Contains(long chatId);
}
