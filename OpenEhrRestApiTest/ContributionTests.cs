using Xunit;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.IO;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Xunit.Abstractions;

namespace OpenEhrRestApiTest
{
    public class ContributionTests : IClassFixture<OpenEhrRestApiTestFixture>
    {

        private string Url = "composition";
        private readonly HttpClient _client;
        private readonly string _basePath;
        private string _testEhrId; 
        private readonly ITestOutputHelper _output;

        public ContributionTests(OpenEhrRestApiTestFixture fixture, ITestOutputHelper output){
            _client = fixture.Client;
            _basePath = fixture.Path;
            _output = output;
            _testEhrId = fixture.TestEhrId;
        }


        // Note that the ehdId below must be present on the test server. Future
        // versions will require to intialize the test server with test EHRs.
        [Theory]
        [InlineData("John Doe", "532")] // lifecycle state 523 = complete
        public async Task Post_CreateNewCompositionShouldReturnSuccess(string committerName, string lifecycle_state){
            Url = "ehr/" + _testEhrId + "/composition";
            var content = Tests.GetTestEhrComposition(_basePath);

           var response = await _client.PostAsync(Url, content);

            if ((int)response.StatusCode != StatusCodes.Status201Created)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _output.WriteLine(responseBody);
            }

            Assert.Equal(StatusCodes.Status201Created, (int) response.StatusCode);
        }

        [Fact]
        public async Task Post_CreateNewCompositionWithInvalidCompositionShouldReturnBadRequest(){
            Url = "ehr/" + _testEhrId + "/composition";
            string composition = @"{""_type"":""XYZ"",""value"":""Vital signs""";

            var content = new StringContent(composition, Encoding.UTF8, "application/json");
            Tests.AddMandatoryOpenEhrRestApiHeaders(content);
            var response = await _client.PostAsync(Url, content);

            Assert.Equal(StatusCodes.Status400BadRequest, (int) response.StatusCode);
        }

        [Fact]
        public async Task Post_CreateNewCompositionWithInvalidEhrIdShouldReturnNotFound(){
            var ehrId = "invalid_Ehr_ID";
            Url = "ehr/" + ehrId + "/composition";
            string composition = @"{""_type"":""XYZ"",""value"":""Vital signs""";

            var content = new StringContent(composition, Encoding.UTF8, "application/json");
            Tests.AddMandatoryOpenEhrRestApiHeaders(content);
            var response = await _client.PostAsync(Url, content);

            var responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseBody);

            Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
        }

        [Fact]
        public async Task Post_CreateTwoIdenticalCompositionsShouldReturnBadRequest()
        {
            Url = "ehr/" + _testEhrId + "/composition";
            var content = Tests.GetTestEhrComposition(_basePath);

            var response = await _client.PostAsync(Url, content);

            string responseBody;
            responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseBody);

            Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);

            response = await _client.PostAsync(Url, content);
            responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseBody);
            
            Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);

        }
    }

}


//# EhrCraft Server Endpoints 
//- Create EHR, /api/v1/ehr/ --> http post /ehr (not put)
//- Get Version /api/v1/server/version --> ???
//- Load Template Ids /api/v1/template ---> http get /template
//- Import template /api/v1/template ---> http post /template
//- Load template /api/v1/template/{template_id}/opt ---> get /template/{template_id}
//- Save composition /api/v1/contribution
//- Commit contriution /api/v1/{ehr_id}/contribution?committerName=.. --> POST {ehr_id}/contribution
//- RunAqlAsync /avpi/v1/query... --> POST /aql/ ... 
//- ValidateAqlAsync /api/v1/query/validate ... --> (not in the ehr rest spec)

