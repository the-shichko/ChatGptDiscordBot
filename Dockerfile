﻿FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ChatGptDiscordBot/ChatGptDiscordBot.csproj", "ChatGptDiscordBot/"]
RUN dotnet restore "ChatGptDiscordBot/ChatGptDiscordBot.csproj"
COPY . .
WORKDIR "/src/ChatGptDiscordBot"
RUN dotnet build "ChatGptDiscordBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatGptDiscordBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatGptDiscordBot.dll"]
