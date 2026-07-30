<#
.SYNOPSIS
    Applies pending EF Core migrations to the database.
.EXAMPLE
    .\ef-update.ps1
    .\ef-update.ps1 InitialCreate   # roll the database back/forward to a specific migration
#>
param(
    [string]$MigrationName
)

dotnet ef database update $MigrationName `
    --project src\FarmApp.Infrastructure `
    --startup-project src\FarmApp.Api
