FROM mcr.microsoft.com/dotnet/core/runtime:3.1

WORKDIR /app

COPY bin/Release/netcoreapp3.1/publish ./

ENTRYPOINT ["dotnet", "JsonConsole.Extensions.Logging.Cli.dll"]
