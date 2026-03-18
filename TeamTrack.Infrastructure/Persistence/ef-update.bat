@echo off
set migration=%1
set base=%~dp0

if "%migration%"=="" (
  dotnet ef database update ^
    --project "%base%.." ^
    --startup-project "%base%..\..\TeamTrack.API"
) else (
  dotnet ef database update %migration% ^
    --project "%base%.." ^
    --startup-project "%base%..\..\TeamTrack.API"
)