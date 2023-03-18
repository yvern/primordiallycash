FROM mcr.microsoft.com/dotnet/sdk:7.0 as dotnet-sdk
RUN apt-get update && apt-get install make

FROM dotnet-sdk as deps
WORKDIR /app
COPY primordiallycash.fsproj .
RUN dotnet restore

FROM deps as test
COPY users.csv .
COPY Library.fs .
COPY Test.fs .
COPY Program.fs .
RUN dotnet test

FROM test as build
COPY Makefile .
RUN make publish

FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /app
COPY --from=build /app/bin/Release/net7.0/linux-x64/publish/primordiallycash .
ARG schedule=users.csv
ADD $schedule .
ENTRYPOINT ["./primordiallycash"]