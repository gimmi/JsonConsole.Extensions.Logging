### Make NuGet

```
dotnet pack `
    --configuration Release `
    -p:Version=0.2.0-beta `
    .\src\JsonConsole.Extensions.Logging\JsonConsole.Extensions.Logging.csproj
```

### Build CLI tool 

```
dotnet publish -c Release .\src\JsonConsole.Extensions.Logging.Cli
docker build --pull --tag jsonconsole:latest .\src\JsonConsole.Extensions.Logging.Cli
```

Run interactively

```
docker run -it --rm jsonconsole:latest
```

Run benchmarks

```
dotnet run --configuration Release --project .\src\JsonConsole.Extensions.Logging.Benchmarks -- --filter SingleLogEntry
```
