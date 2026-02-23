namespace PillsReminderTgBot.WebApi.Services;

public interface IReminderAcknowledgementStore
{
    bool IsConfirmed(Guid cycleId);
    void Confirm(Guid cycleId);
}
