<#
.SYNOPSIS
    Removes the most recent (not-yet-applied, or already-rolled-back) EF Core migration.
.EXAMPLE
    .\ef-remove.ps1
#>

dotnet ef migrations remove `
    --project src\FarmApp.Infrastructure `
    --startup-project src\FarmApp.Api
