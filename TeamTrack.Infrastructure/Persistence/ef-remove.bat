@echo off
set base=%~dp0

dotnet ef migrations remove ^
  --project "%base%.." ^
  --startup-project "%base%..\..\TeamTrack.API"