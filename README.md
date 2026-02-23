# PillsReminderTgBot

Телеграм-бот для напоминаний о приеме таблеток по расписанию. Сейчас: ежедневно в 07:00 (MSK), повторы в течение 3 часов каждые 30 минут, до подтверждения кнопкой «Я выпил».

## Быстрый старт (Windows)
1) Создай `.env` на основе `.env.example` и заполни `Telegram__BotToken`.
2) Запусти проект:

```
C:\Users\Dmitri\.dotnet\dotnet.exe run --project .\src\PillsReminderTgBot.WebApi
```

## Docker (Linux)
1) Создай `.env` на основе `.env.example` и заполни `Telegram__BotToken`.
2) Запусти контейнер:

```
docker compose up -d --build
```

## Команды бота
- `/start` — включить напоминания
- `/stop` — отключить напоминания
- `/status` — текущий статус
- `/help` — помощь
- `/test` — тестовое напоминание

## BotFather
В BotFather выполни `/setcommands` и укажи список команд:

```
start - включить напоминания
stop - отключить напоминания
status - текущий статус
help - помощь
test - тестовое напоминание
```

## Health check
```
GET /health
```
Пример:
```
curl http://localhost:8080/health
```

## Конфигурация
Базовые значения в `src/PillsReminderTgBot.WebApi/appsettings.json`. Переопределение через переменные окружения:

- `Telegram__BotToken`
- `Reminder__TimeZoneId`
- `Reminder__DailyTime`
- `Reminder__Repeat__Window`
- `Reminder__Repeat__Interval`
