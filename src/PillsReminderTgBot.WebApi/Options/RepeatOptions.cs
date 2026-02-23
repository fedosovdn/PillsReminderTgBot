using System.ComponentModel.DataAnnotations;

namespace PillsReminderTgBot.WebApi.Options;

public sealed class RepeatOptions
{
    [Required]
    public TimeSpan Window { get; init; } = TimeSpan.FromHours(3);

    [Required]
    public TimeSpan Interval { get; init; } = TimeSpan.FromMinutes(30);
}
