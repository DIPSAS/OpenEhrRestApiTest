using Xunit;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using Xunit.Abstractions;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;

namespace OpenEhrRestApiTest
{
    public class QueryTests : IClassFixture<OpenEhrRestApiTestFixture>
    {
        private string Url = "query";
        private readonly HttpClient _client;
        private string _testEhrId;

        private readonly ITestOutputHelper _output;

        public QueryTests(OpenEhrRestApiTestFixture fixture, ITestOutputHelper output){
            _client = fixture.Client;
            _output = output; 
        }

        [Theory]
        [InlineData(0,0)]
        [InlineData(1,1)]
        public async Task Post_ExecuteAValidAQLQueryReturnsSuccess(int fetch, int offset){
            Url += "/aql";

            var aql = "SELECT c FROM COMPOSITION c";
            JObject query = new JObject();
            query["q"] = aql;
            query["offset"] = offset; 
            query["fetch"] = fetch;

            var content = new StringContent(query.ToString());
            Tests.AddMandatoryOpenEhrRestApiHeaders(content);

            var response = await _client.PostAsync(Url, content);

            Assert.Equal(StatusCodes.Status200OK, (int) response.StatusCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData("asdf")]
        public async Task Post_ExececuteInvalidAQLQueryReturnsBadRequest(string aql){
            
            Url += "/aql";

            JObject query = new JObject();
            query["q"] = aql;

            var content = new StringContent(query.ToString());
            Tests.AddMandatoryOpenEhrRestApiHeaders(content);

            var response = await _client.PostAsync(Url, content);

            Assert.Equal(StatusCodes.Status400BadRequest, (int) response.StatusCode);
        }

        [Fact]
        public async Task Get_ExecuteValidAQLQueryReturnsSuccess(){
            Url += "?aql={aql}";
            throw new NotImplementedException();
        }
    }
}
