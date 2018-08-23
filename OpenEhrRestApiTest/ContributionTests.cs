using Xunit;
using System.Threading.Tasks;
using System.Net.Http;
using System;

namespace OpenEhrRestApiTest
{
    public class ContributionTests : IClassFixture<OpenEhrRestApiTestFixture>
    {

        private const string Url = "composition";
        private readonly HttpClient _client;
        public ContributionTests(OpenEhrRestApiTestFixture fixture){
            _client = fixture.Client;
        }

        [Fact]
        public async Task Post_CreateNewCompositionShouldReturnSuccess(){
            throw new NotImplementedException();
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

