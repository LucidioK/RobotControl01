dotnet restore
dotnet build   --framework net5.0 --runtime win-x64 --no-restore
dotnet publish --framework net5.0 --runtime win-x64 --no-build   --self-contained
