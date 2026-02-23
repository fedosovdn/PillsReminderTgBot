namespace PillsReminderTgBot.WebApi.Services;

public interface IReminderStatusService
{
    string GetStatus(long chatId);
}
