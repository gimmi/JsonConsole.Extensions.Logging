```
dotnet publish -c Release .\src\JsonConsole.Extensions.Logging.Cli\JsonConsole.Extensions.Logging.Cli.csproj
docker build --pull --tag jsonconsole:latest .\src\JsonConsole.Extensions.Logging.Cli
```

Run interactively

```
docker run -it --rm jsonconsole
```
