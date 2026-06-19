$ErrorActionPreference = "Stop"
dotnet build .\samples\Sample.PlaywrightTests\Sample.PlaywrightTests.csproj
pwsh .\samples\Sample.PlaywrightTests\bin\Debug\net8.0\playwright.ps1 install --with-deps
