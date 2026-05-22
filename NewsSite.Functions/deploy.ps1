param(
    [Parameter(Mandatory = $true)]
    [string]$FunctionAppName,

    [Parameter(Mandatory = $true)]
    [string]$ResourceGroup
)

$ErrorActionPreference = 'Stop'

Write-Host "Building Azure Functions project..."
dotnet publish .\NewsSite.Functions\NewsSite.Functions.csproj -c Release

Write-Host "Deploying to Azure Function App '$FunctionAppName'..."
func azure functionapp publish $FunctionAppName --dotnet-isolated

Write-Host "Deployment completed."
Write-Host "Verify application settings in resource group '$ResourceGroup'."
