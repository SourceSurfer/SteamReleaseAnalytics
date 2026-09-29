# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SteamReleaseAnalytics.sln", "."]
COPY ["SteamReleaseAnalytics.Api/SteamReleaseAnalytics.Api.csproj", "SteamReleaseAnalytics.Api/"]
COPY ["SteamReleaseAnalytics.Core/SteamReleaseAnalytics.Core.csproj", "SteamReleaseAnalytics.Core/"]
COPY ["SteamReleaseAnalytics.Infrastructure/SteamReleaseAnalytics.Infrastructure.csproj", "SteamReleaseAnalytics.Infrastructure/"]
COPY ["SteamReleaseAnalytics.Services/SteamReleaseAnalytics.Services.csproj", "SteamReleaseAnalytics.Services/"]
COPY ["tests/SteamReleaseAnalytics.Tests/SteamReleaseAnalytics.Tests.csproj", "tests/SteamReleaseAnalytics.Tests/"]

RUN dotnet restore "SteamReleaseAnalytics.sln"

COPY . .

RUN dotnet build "SteamReleaseAnalytics.sln" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "SteamReleaseAnalytics.Api/SteamReleaseAnalytics.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "SteamReleaseAnalytics.Api.dll"]