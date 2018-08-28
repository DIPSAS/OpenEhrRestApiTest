using Xunit;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.IO;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace OpenEhrRestApiTest
{
    public class ContributionTests : IClassFixture<OpenEhrRestApiTestFixture>
    {

        private string Url = "composition";
        private readonly HttpClient _client;

        private readonly string _basePath;

        public ContributionTests(OpenEhrRestApiTestFixture fixture){
            _client = fixture.Client;
            _basePath = fixture.Path;
        }


        // Note that the ehdId below must be present on the test server. Future
        // versions will require to intialize the test server with test EHRs.
        [Theory]
        [InlineData("John Doe", "532")] // lifecycle state 523 = complete
        public async Task Post_CreateNewCompositionShouldReturnSuccess(string committerName, string lifecycle_state){
            var ehrId = "05fad39b-ecde-4bfe-92ad-cd1accc76a14"; 
            Url = "ehr/" + ehrId + "/composition";
            string composition = TestComposition();

            var content = new StringContent(composition, Encoding.UTF8, "application/json");
            Tests.AddMandatoryOpenEhrRestApiHeaders(content, committerName, lifecycle_state);
            var response = await _client.PostAsync(Url, content);

            Assert.Equal(StatusCodes.Status201Created, (int) response.StatusCode);
        }

        [Fact]
        public async Task Post_CreateNewCompositionWithInvalidCompositionShouldReturnBadRequest(){
            var ehrId = "05fad39b-ecde-4bfe-92ad-cd1accc76a14"; 
            Url = "ehr/" + ehrId + "/composition";
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

            Assert.Equal(StatusCodes.Status404NotFound, (int) response.StatusCode);
        }    

        private string TestComposition(){
            return System.IO.File.ReadAllText(Path.Combine(_basePath, "TestData/example-composition.json"));
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

