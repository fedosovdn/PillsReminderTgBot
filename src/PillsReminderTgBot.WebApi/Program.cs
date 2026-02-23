using Microsoft.Extensions.Options;
using PillsReminderTgBot.WebApi.Options;
using PillsReminderTgBot.WebApi.Services;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOptions<ReminderOptions>()
    .Bind(builder.Configuration.GetSection(ReminderOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<TelegramOptions>()
    .Bind(builder.Configuration.GetSection(TelegramOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ITimeZoneResolver, TimeZoneResolver>();
builder.Services.AddSingleton<IReminderScheduleCalculator, ReminderScheduleCalculator>();
builder.Services.AddSingleton<IReminderRecipientStore, InMemoryReminderRecipientStore>();
builder.Services.AddSingleton<IReminderAcknowledgementStore, InMemoryReminderAcknowledgementStore>();
builder.Services.AddSingleton<IReminderStatusService, ReminderStatusService>();
builder.Services.AddSingleton<IReminderSender, TelegramReminderSender>();
builder.Services.AddHostedService<ReminderBackgroundService>();
builder.Services.AddSingleton<ITelegramUpdateHandler, TelegramUpdateHandler>();
builder.Services.AddHostedService<TelegramPollingService>();
builder.Services.AddSingleton<ITelegramBotClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;
    return new TelegramBotClient(options.BotToken);
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("ok"));

app.Run();
