# run-dev.ps1
# Script to run both Spendly.Api and Spendly.Web simultaneously

Write-Host "Starting Spendly Development Environment..." -ForegroundColor Green

# Start the API in the background
$apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --project .\src\Spendly.Api\Spendly.Api.csproj --launch-profile https" -PassThru -NoNewWindow

# Wait a second to let the API start
Start-Sleep -Seconds 2

# Start the Web project in the background
$webProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --project .\src\Spendly.Web\Spendly.Web.csproj --launch-profile https" -PassThru -NoNewWindow

Write-Host "Both projects are starting. To stop them, press Enter." -ForegroundColor Cyan
Read-Host

Write-Host "Stopping projects..." -ForegroundColor Yellow
Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
Stop-Process -Id $webProcess.Id -Force -ErrorAction SilentlyContinue
Write-Host "Done." -ForegroundColor Green
