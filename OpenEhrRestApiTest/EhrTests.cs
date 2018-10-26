using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OpenEhrRestApiTest
{
    public class EhrControllerTests : IClassFixture<OpenEhrRestApiTestFixture>
    {
        private const string Url = "ehr";
        private static HttpClient _client;
        private static string _path;
        private readonly ITestOutputHelper _output;
        private string _testEhrId;

        public EhrControllerTests(OpenEhrRestApiTestFixture fixture, ITestOutputHelper output)
        {
            _client = fixture.Client;
            _path = fixture.Path;
            _output = output;
            _testEhrId = fixture.TestEhrId;
        }


        // Note that the below ID is found in the current test server, and will
        // not work on other servers. Future versions should ensure that the
        // test server is populated with test EHRs.
        [Fact]
        public async Task Get_EhrWithValidEhrIdShouldReturnOk()
        {
            var response = await _client.GetAsync(Url + "/" + _testEhrId);

            if ((int)response.StatusCode != StatusCodes.Status200OK)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _output.WriteLine(responseBody);
            }
            Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        }


        [Theory]
        [InlineData("")]
        [InlineData("100")]
        [InlineData("-1")]
        public async Task Get_EhrWithNonExistingEhrIdShouldReturnNotFound(string ehrId)
        {
            var response = await _client.GetAsync(Url + "/" + ehrId);

            if ((int)response.StatusCode != StatusCodes.Status404NotFound)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _output.WriteLine(responseBody);
            }

            Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
        }

        [Theory(Skip = "Not obvious as of 20.08.2018 in the REST API spec what should return a HTTP bad request.")]
        [InlineData("")]
        public async Task Get_EhrWithInvalidEhrIdShouldReturnBadRequest(string ehrId)
        {
            var response = await _client.GetAsync(Url + ehrId);

            if ((int)response.StatusCode != StatusCodes.Status400BadRequest)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _output.WriteLine(responseBody);
            }

            Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        }

        [Fact]
        public async Task Post_CreateNewEhrReturnsCreated()
        {
            var content = Tests.GetTestEhrPostContent(_path);
            var response = await _client.PostAsync(Url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if ((int)response.StatusCode != StatusCodes.Status201Created)
            {
                _output.WriteLine(responseBody);
            }

            JObject ehr = JObject.Parse(responseBody);

            Assert.True(response.Headers.Contains("Location"), "Response header must contain Location.");
            Assert.True(response.Content.Headers.Contains("Content-Type"), "Response header must contain Content-Type");

            Assert.True(ehr.ContainsKey("system_id"), "Resulting JSON should contain system_id");
            Assert.True(ehr.ContainsKey("ehr_id"), "Resulting JSON should contain ehr_id");
            Assert.True(ehr.ContainsKey("ehr_status"), "Resulting JSON should contain ehr_status");
            Assert.True(ehr.ContainsKey("time_created"), "Resulting JSON should contain time_created");
            Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);
        }
    }
}