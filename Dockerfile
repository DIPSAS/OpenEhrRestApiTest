FROM microsoft/dotnet:2.1-sdk

COPY . /OpenEhrRestApiTest
WORKDIR /OpenEhrRestApiTest

CMD dotnet test --logger:"console;verbosity=normal"
