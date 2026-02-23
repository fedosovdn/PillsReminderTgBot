using System.ComponentModel.DataAnnotations;

namespace PillsReminderTgBot.WebApi.Options;

public sealed class TelegramOptions
{
    public const string SectionName = "Telegram";

    [Required]
    public string BotToken { get; init; } = string.Empty;
}
