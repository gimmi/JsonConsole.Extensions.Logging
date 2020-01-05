### Introduction

This is a simple `Microsoft.Extensions.Logging` implementation that write JSON to the console.

The main use case for this library is to use it in Dockerized application, and let Docker runtime route stdout to something like fluentd for further aggregation/processing.

### Installation and usage

Install it from NuGet: https://www.nuget.org/packages/JsonConsole.Extensions.Logging/

### License

[MIT License](LICENSE)

### Contribute

##### Build the NuGet package

```
dotnet pack `
    --configuration Release `
    -p:Version=0.1.0-pre `
    .\src\JsonConsole.Extensions.Logging\JsonConsole.Extensions.Logging.csproj
```

##### Build the CLI tool

```
dotnet publish -c Release .\src\JsonConsole.Extensions.Logging.Cli
docker build --pull --tag jsonconsole:latest .\src\JsonConsole.Extensions.Logging.Cli
```

##### Run the cli tool in Docker

```
docker run -it --rm jsonconsole:latest
```

##### Run benchmarks

```
dotnet run `
    --configuration Release `
    --project .\src\JsonConsole.Extensions.Logging.Benchmarks `
    -- `
    --filter SingleLogEntry
```
