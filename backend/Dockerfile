# backend/Dockerfile

# ── Stage 1: Build (SDK) ──────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY ["Blinder.slnx", "./"]
COPY ["Blinder.Api/Blinder.Api.csproj", "Blinder.Api/"]
COPY ["Blinder.Tests/Blinder.Tests.csproj", "Blinder.Tests/"]
RUN dotnet restore "Blinder.Api/Blinder.Api.csproj"

# Copy remaining source and publish
COPY . .
WORKDIR /src/Blinder.Api
RUN dotnet publish "Blinder.Api.csproj" -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime (no SDK, no dotnet-ef) ──────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Blinder.Api.dll"]
