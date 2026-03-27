# backend/Blinder.IdentityServer/Dockerfile

# ── Stage 1: Build (SDK) ──────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching.
# IdentityServer references Blinder.Api — both .csproj files must be present for restore.
COPY ["Blinder.slnx", "./"]
COPY ["Blinder.Api/Blinder.Api.csproj", "Blinder.Api/"]
COPY ["Blinder.IdentityServer/Blinder.IdentityServer.csproj", "Blinder.IdentityServer/"]
RUN dotnet restore "Blinder.IdentityServer/Blinder.IdentityServer.csproj"

# Copy remaining source and publish
COPY . .
WORKDIR /src/Blinder.IdentityServer
RUN dotnet publish "Blinder.IdentityServer.csproj" -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime (no SDK, no dotnet-ef) ──────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Blinder.IdentityServer.dll"]
