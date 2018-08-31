using Xunit;
using System.Threading.Tasks;
using System.Net.Http;
using System;

namespace OpenEhrRestApiTest
{
    public class QueryTests : IClassFixture<OpenEhrRestApiTestFixture>
    {
        private string Url = "query";
        private readonly HttpClient _client;
        public QueryTests(OpenEhrRestApiTestFixture fixture){
            _client = fixture.Client;
        }

//        [Fact]
        public async Task Post_ExecuteAValidAQLQueryReturnsSuccess(){
            Url += "/aql";
            throw new NotImplementedException();
        }

//        [Fact]
        public async Task Get_ExecuteValidAQLQueryReturnsSuccess(){
            Url += "?aql={aql}";
            throw new NotImplementedException();
        }
    }
}
