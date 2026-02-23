FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/PillsReminderTgBot.WebApi/PillsReminderTgBot.WebApi.csproj", "src/PillsReminderTgBot.WebApi/"]
RUN dotnet restore "src/PillsReminderTgBot.WebApi/PillsReminderTgBot.WebApi.csproj"
COPY . .
WORKDIR "/src/src/PillsReminderTgBot.WebApi"
RUN dotnet build "./PillsReminderTgBot.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PillsReminderTgBot.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PillsReminderTgBot.WebApi.dll"]
