FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Blinder.AdminPanel/Blinder.AdminPanel.csproj", "src/Blinder.AdminPanel/"]
COPY ["src/Blinder.Contracts/Blinder.Contracts.csproj", "src/Blinder.Contracts/"]
COPY ["src/Blinder.SharedKernel/Blinder.SharedKernel.csproj", "src/Blinder.SharedKernel/"]
COPY ["Directory.Build.props", "./"]
RUN dotnet restore "src/Blinder.AdminPanel/Blinder.AdminPanel.csproj"

COPY . .
RUN dotnet publish "src/Blinder.AdminPanel/Blinder.AdminPanel.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

RUN apt-get update \
  && apt-get install -y --no-install-recommends curl \
  && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Blinder.AdminPanel.dll"]
