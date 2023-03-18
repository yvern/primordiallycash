schedule ?= users.csv

publish:
	@-rm primordiallycash
	dotnet publish \
    		 -c Release \
    		 --use-current-runtime \
    		 --self-contained true \
    		 -p:PublishReadyToRun=true \
    		 -p:PublishTrimmed=true \
    		 -p:PublishSingleFile=true
	@cp ./bin/Release/net7.0/*/publish/primordiallycash .

build-img:
	docker build -t primordiallycash --build-arg schedule=$(schedule) .