using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

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

        [Theory]
        [InlineData("select c/uid/value as UID, c/name/value as Name from COMPOSITION c where c/name/value = $cName", "cName", "Vital Signs")]
        public async Task Post_ExecuteAValidAQLQueryWithParametersReturnsSuccess(string aql, string parameterKey, string parameterValue)
        {
            Url += "/aql";
            var fetch = 0;
            var offset = 0;

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters[parameterKey] = parameterValue;

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

        [Theory]
        [InlineData("select c from composition c")]
        [InlineData("select c from composition c", 1)]
        [InlineData("select c from composition c", 1, 2)]
        public async Task Get_ExecuteValidAQLQueryReturnsSuccess(string aql, int fetch = 0, int offset = 0)
        {
            Url += $"/aql?q={aql}";
            if (fetch != 0)
            {
                Url = Url + "&fetch=" + fetch;
            }

            if (offset != 0)
            {
                Url = Url + "&offset=" + offset;
            }

            var response = await _client.GetAsync(Url);

            Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

            if (fetch != 0)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JObject.Parse(responseBody);
                Assert.Equal(fetch, responseObject["totalResults"]);
            }

        }

        [Theory]
        [InlineData("select")]
        [InlineData("invalid query")]
        public async Task Get_ExecuteInvalidAQLQueryReturnsBadRequest(string aql)
        {
            Url += $"/aql?q={aql}";

            var response = await _client.GetAsync(Url);

            Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        }
    }
}
