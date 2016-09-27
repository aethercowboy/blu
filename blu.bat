@echo off
pushd src\blu
dotnet run --project project.json %*
popd