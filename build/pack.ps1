$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$out = Join-Path $root "artifacts/packages"
New-Item -ItemType Directory -Force -Path $out | Out-Null
dotnet pack (Join-Path $root "src/LwX.Web.Testing/LwX.Web.Testing.csproj") -c Release -o $out
dotnet pack (Join-Path $root "src/LwX.Web.Testing.Xunit/LwX.Web.Testing.Xunit.csproj") -c Release -o $out
