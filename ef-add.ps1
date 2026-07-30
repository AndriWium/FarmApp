<#
.SYNOPSIS
    Adds a new EF Core migration.
.EXAMPLE
    .\ef-add.ps1 AddCropAndBlock
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$Name
)

dotnet ef migrations add $Name `
    --project src\FarmApp.Infrastructure `
    --startup-project src\FarmApp.Api
