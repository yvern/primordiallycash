schedule ?= users.csv

publish:
	dotnet publish \
    -c Release \
    --use-current-runtime \
    --self-contained true \
    -p:PublishReadyToRun=true \
    -p:PublishTrimmed=true \
    -p:PublishSingleFile=true

build-img:
	docker build -t primordiallycash --build-arg schedule=$(schedule) .