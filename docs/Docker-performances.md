### Performances in Docker environment

##### Run the benchmarks on Windows console

```
dotnet publish -c Release .\src\JsonConsole.Extensions.Logging.Cli
dotnet .\src\JsonConsole.Extensions.Logging.Cli\bin\Release\netcoreapp3.1\JsonConsole.Extensions.Logging.Cli.dll bench --resultpath .\windows-result.txt --count 100000
type .\windows-result.txt

Written 100000 logs in 00:00:26.9423015
```

##### Run the benchmarks on Docker interactive

```
dotnet publish -c Release .\src\JsonConsole.Extensions.Logging.Cli
docker build --pull --tag jsonconsole:latest .\src\JsonConsole.Extensions.Logging.Cli
docker run -it --rm `
    -v C:/Users/gimmi/source/JsonConsole.Extensions.Logging:/out `
    jsonconsole:latest `
    bench --resultpath /out/docker-it-result.txt --count 100000
type .\docker-it-result.txt

Written 100000 logs in 00:00:05.3857195
```

##### Run the benchmarks on Docker detached

```
dotnet publish -c Release .\src\JsonConsole.Extensions.Logging.Cli
docker build --pull --tag jsonconsole:latest .\src\JsonConsole.Extensions.Logging.Cli
docker run --rm -d `
    -v C:/Users/gimmi/source/JsonConsole.Extensions.Logging:/out `
    jsonconsole:latest `
    bench --resultpath /out/docker-detached-result.txt --count 100000
type .\docker-detached-result.txt

Written 100000 logs in 00:00:01.4329790
```

##### Run the benchmarks on Docker detached with null log

```
dotnet publish -c Release .\src\JsonConsole.Extensions.Logging.Cli
docker build --pull --tag jsonconsole:latest .\src\JsonConsole.Extensions.Logging.Cli
docker run --rm -d `
    --log-driver none `
    -v C:/Users/gimmi/source/JsonConsole.Extensions.Logging:/out `
    jsonconsole:latest `
    bench --resultpath /out/docker-none-result.txt --count 100000
type .\docker-none-result.txt

Written 100000 logs in 00:00:01.5376707
```
