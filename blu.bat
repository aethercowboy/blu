@echo off
dotnet build blu.sln > nul
dotnet run --project src\blu\blu.csproj %*