<#
.SYNOPSIS
    Lists all EF Core migrations and shows which have been applied to the database.
.EXAMPLE
    .\ef-list.ps1
#>

dotnet ef migrations list `
    --project src\FarmApp.Infrastructure `
    --startup-project src\FarmApp.Api
