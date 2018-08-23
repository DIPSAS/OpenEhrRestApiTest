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
        [Fact]
        public async Task Post_CreateNewCompositionShouldReturnSuccess(){
            var ehrId = "05fad39b-ecde-4bfe-92ad-cd1accc76a14"; 

            Url = "ehr/" + ehrId + "/compositions";
            string composition = System.IO.File.ReadAllText(Path.Combine(_basePath, "TestData/CompositionWithObservations.xml"));

            var content = new StringContent(composition, Encoding.UTF8, "application/xml");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

            var response = await _client.PostAsync(Url, content);

            Assert.Equal(StatusCodes.Status200OK, (int) response.StatusCode);
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

