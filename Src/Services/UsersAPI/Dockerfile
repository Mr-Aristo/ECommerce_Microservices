FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Src/Services/UsersAPI/UsersAPI.csproj", "Src/Services/UsersAPI/"]
COPY ["Src/BuildingBlocks/BuildingBlock/BuildingBlock.csproj", "Src/BuildingBlocks/BuildingBlock/"]
RUN dotnet restore "./Src/Services/UsersAPI/UsersAPI.csproj"
COPY . .
WORKDIR "/src/Src/Services/UsersAPI"
RUN dotnet build "./UsersAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UsersAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UsersAPI.dll"]
