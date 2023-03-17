FROM mcr.microsoft.com/dotnet/sdk:7.0 as dotnet-sdk
WORKDIR /app
COPY primordiallycash.fsproj .
RUN dotnet restore

FROM dotnet-sdk as test
COPY users.csv .
COPY Library.fs .
COPY Test.fs .
COPY Program.fs .
RUN dotnet test

FROM test as build
RUN dotnet publish \
    -c Release \
    --use-current-runtime \
    --self-contained true \
    -p:PublishReadyToRun=true \
    -p:PublishTrimmed=true \
    -p:PublishSingleFile=true

FROM mcr.microsoft.com/dotnet/runtime:7.0 as app
WORKDIR /app
COPY --from=build /app/bin/Release/net7.0/linux-x64/publish/primordiallycash .
ENTRYPOINT ./primordiallycash