@echo off
dotnet build blu.sln
dotnet run --project src\blu\blu.csproj %*