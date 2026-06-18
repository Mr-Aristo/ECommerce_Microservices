FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Src/Services/NotificationAPI/NotificationAPI.csproj", "Src/Services/NotificationAPI/"]
COPY ["Src/BuildingBlocks/BuildingBlock/BuildingBlock.csproj", "Src/BuildingBlocks/BuildingBlock/"]
COPY ["Src/BuildingBlocks/BuildingBlockMessaging/BuildingBlockMessaging.csproj", "Src/BuildingBlocks/BuildingBlockMessaging/"]
RUN dotnet restore "./Src/Services/NotificationAPI/NotificationAPI.csproj"
COPY . .
WORKDIR "/src/Src/Services/NotificationAPI"
RUN dotnet build "./NotificationAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NotificationAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationAPI.dll"]
