FROM microsoft/dotnet:2.1-sdk

COPY NuGet.Config /root/.nuget/NuGet/

COPY . /OpenEhrRestApiTest
WORKDIR /OpenEhrRestApiTest

CMD dotnet test

