# openEHR REST API TEST
Test utility to verify the correctness of different openEHR REST API
implementations. See the 
[openEHR specification]( https://www.openehr.org/programs/specification/workingbaseline)
for more information.

Could be used as a .NET test project in existing implementations by adding it
as a git submodule.

# Run
## Docker
We automatically build a [Docker](http://docker.io) image no the [Docker Hub](https://hub.docker.com/r/dipsas/openehr-rest-test) 
with the tests from this repository. Users with Docker installed can run these
with a single command: 

```
 docker run -e ServerHostname='host.docker.internal' -t dipsas/openehr-rest-test
 ```

### Configuration 
Users can specify the hostname and ports using the envionrment variables
`ServerHostname` and `ServerPort`. The above command with
`ServerHostname='host.docker.internal'` will run tests against an openEHR REST
API running on the docker host. 

## Local
It is also possible to run the tests locally without Docker. 

```
 dotnet test
```

### Configuration 
It's possible to target differet openEHR REST API servers by modifying the
[settings.json](OpenEhrRestApiTest/settings.json) file:

```
{
    "ServerHostname": "localhost",
    "ServerPort":  "9000",
    "Protocol": "http",
    "BasePath": ""
}
```

It is also possible to set these using environment variables. Setting the
`BasePath` will prepend this path to the URL paths used in the tests. E.g.
using the above settings will test an openEHR REST API at
`http://localhost:9000/`, and setting e.g. `BasePath="/openehr/"` will test an
openEHR REST API at `http://localhost:9000/openehr/`.

# Test Data
Tests that require input data, such as creating a new composition, use test
data in [TestData](OpenEhrRestApiTest/TestData). These datasets should be
identical to the test data in the REST API Swagger docs.

# Build Status
[![Docker Build Status](https://img.shields.io/docker/build/dipsas/openehr-rest-test.svg)](https://hub.docker.com/r/dipsas/openehr-rest-test/)
