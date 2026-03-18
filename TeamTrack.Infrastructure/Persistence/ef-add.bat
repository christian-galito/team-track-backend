@echo off
set name=%1

if "%name%"=="" (
  echo Please provide a migration name.
  exit /b 1
)

set base=%~dp0

dotnet ef migrations add %name% ^
  --project "%base%.." ^
  --startup-project "%base%..\..\TeamTrack.API" ^
  --output-dir Persistence/Migrations