# OpenEHR REST API TEST
Test utility to verify the correctness of different OpenEhr REST API
implementations. See the 
[OpenEhr specification]( https://www.openehr.org/programs/specification/workingbaseline)
for more information.

Could be used as a .NET test project in existing implementations by adding it
as a git submodule.

# Run tests

## Local

```
 dotnet test
```

### Configuration 
It's possible to target differet OpenEHR REST API servers by modifying the
[settings.json](OpenEhrRestApiTest/settings.json) file:

```
{
    "ServerHostname": "localhost",
    "ServerPort":  "9000",
    "Protocol": "http"
}
```

## Docker
First, build the docker image: 

```
 docker build -t openehr-rest-test .
```

then run the container

```
 docker run -e ServerHostname='host.docker.internal' -t openehr-rest-test
 ```

You can specify the hostname and ports using the envionrment variables
`ServerHostname` and `ServerPort`. The above command with
`ServerHostname='host.docker.internal'` will run tests against an OpenEHR REST
API running on the docker host. 


# Test Data
Tests that require input data, such as creating a new composition, use test
data in [TestData](OpenEhrRestApiTest/TestData). These datasets should be
identical to the test data in the REST API Swagger docs.
