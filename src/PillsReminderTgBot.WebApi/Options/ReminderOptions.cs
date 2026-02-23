using System.ComponentModel.DataAnnotations;

namespace PillsReminderTgBot.WebApi.Options;

public sealed class ReminderOptions
{
    public const string SectionName = "Reminder";

    [Required]
    public string TimeZoneId { get; init; } = "Russian Standard Time";

    [Required]
    public TimeSpan DailyTime { get; init; } = TimeSpan.FromHours(7);

    [Required]
    public RepeatOptions Repeat { get; init; } = new();
}
