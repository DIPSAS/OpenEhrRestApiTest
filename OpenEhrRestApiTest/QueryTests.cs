using Xunit;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using Xunit.Abstractions;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace OpenEhrRestApiTest
{
    public class QueryTests : IClassFixture<OpenEhrRestApiTestFixture>
    {
        private string Url = "query";
        private readonly HttpClient _client;
        private string _testEhrId;

        private readonly ITestOutputHelper _output;

        public QueryTests(OpenEhrRestApiTestFixture fixture, ITestOutputHelper output)
        {
            _client = fixture.Client;
            _output = output;
            _testEhrId = fixture.TestEhrId;
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(10, 0)]
        public async Task Post_ExecuteAValidAQLQueryReturnsSuccess(int fetch, int offset)
        {
            Url += "/aql";

            var query = Tests.CreateTestAqlQuery(fetch, offset);
            var content = new StringContent(query.ToString());
            Tests.AddMandatoryOpenEhrRestApiHeaders(content);

            var response = await _client.PostAsync(Url, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseBody);
            JObject result = JObject.Parse(responseBody);

            Assert.NotNull(result["columns"]);
            Assert.NotNull(result["rows"]);
            Assert.NotNull(result["meta"]["_type"]);
            Assert.NotNull(result["meta"]["_schemaVersion"]);
            Assert.NotNull(result["meta"]["_created"]);
            Assert.NotNull(result["meta"]["_generator"]);
            Assert.NotNull(result["meta"]["_executed_aql"]);

            Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        }

        [Fact]
        public async Task Post_ExecuteAValidAQLQueryWithParametersReturnsSuccess()
        {
            Url += "/aql";
            var fetch = 0;
            var offset = 0;

            var aql = "select e from ehr e where e/ehr_id/value = '$ehrId'";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["ehrId"] = _testEhrId;

            var query = Tests.CreateTestAqlQuery(aql, parameters, fetch, offset);

            var content = new StringContent(query.ToString());
            Tests.AddMandatoryOpenEhrRestApiHeaders(content);

            var response = await _client.PostAsync(Url, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseBody);

            JObject result = JObject.Parse(responseBody);

            Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData("asdf")]
        public async Task Post_ExececuteInvalidAQLQueryReturnsBadRequest(string aql)
        {

            Url += "/aql";

            JObject query = new JObject();
            query["q"] = aql;

            var content = new StringContent(query.ToString());
            Tests.AddMandatoryOpenEhrRestApiHeaders(content);

            var response = await _client.PostAsync(Url, content);

            Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        }
    }
}
