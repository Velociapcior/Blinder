# EF Core Migration Workflow

Run all commands from the `backend/` directory.

## Tooling

```powershell
dotnet tool restore
```

## IdentityServer

Inspect the generated SQL before applying it:

```powershell
dotnet ef migrations add InitialIdentitySchema --project src/Blinder.IdentityServer/Blinder.IdentityServer.csproj --startup-project src/Blinder.IdentityServer/Blinder.IdentityServer.csproj --context Blinder.IdentityServer.Persistence.IdentityDbContext --output-dir Persistence/Migrations
dotnet ef migrations script --project src/Blinder.IdentityServer/Blinder.IdentityServer.csproj --startup-project src/Blinder.IdentityServer/Blinder.IdentityServer.csproj --context Blinder.IdentityServer.Persistence.IdentityDbContext
dotnet ef database update --project src/Blinder.IdentityServer/Blinder.IdentityServer.csproj --startup-project src/Blinder.IdentityServer/Blinder.IdentityServer.csproj --context Blinder.IdentityServer.Persistence.IdentityDbContext
```

## Api

Inspect the generated SQL before applying it:

```powershell
dotnet ef migrations add InitialAppSchema --project src/Blinder.Api/Blinder.Api.csproj --startup-project src/Blinder.Api/Blinder.Api.csproj --context Blinder.Api.Persistence.AppDbContext --output-dir Persistence/Migrations
dotnet ef migrations script --project src/Blinder.Api/Blinder.Api.csproj --startup-project src/Blinder.Api/Blinder.Api.csproj --context Blinder.Api.Persistence.AppDbContext
dotnet ef database update --project src/Blinder.Api/Blinder.Api.csproj --startup-project src/Blinder.Api/Blinder.Api.csproj --context Blinder.Api.Persistence.AppDbContext
```

## Local Validation

The PostgreSQL bootstrap script in `../postgres/init/` only runs on a fresh database volume.

```powershell
docker compose down -v
docker compose up -d postgres
```