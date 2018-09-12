using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace OpenEhrRestApiTest
{
    public class ContributionTests : IClassFixture<OpenEhrRestApiTestFixture>
    {
        private string Url = "ehr";
        private readonly HttpClient _client;
        private readonly string _basePath;
        private string _testEhrId;
        private readonly ITestOutputHelper _output;

        public ContributionTests(OpenEhrRestApiTestFixture fixture, ITestOutputHelper output)
        {
            _client = fixture.Client;
            _basePath = fixture.Path;
            _output = output;
            _testEhrId = fixture.TestEhrId;
        }

        [Fact]
        public async Task Post_CreateNewContributionShouldReturnSuccess()
        {
            Url = Url + "/" + _testEhrId + "/contribution";
            var content = Tests.GetTestContribution(_basePath);

            var response = await _client.PostAsync(Url, content);

            var responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseBody);

            JObject contribution = JObject.Parse(responseBody);

            Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);
            Assert.True(response.Headers.Contains("Location"), "Response header must contain Location.");
            Assert.True(response.Headers.Contains("ETag"), "Response header must contain ETag.");
            Assert.NotNull(contribution);
        }

        [Fact]
        public async Task Post_CreateNewContributionWithInvalidVersionsShouldReturnBadRequest()
        {

            Url = Url + "/" + _testEhrId + "/contribution";
            var content = Tests.GetInvalidTestContribution(_basePath);

            var response = await _client.PostAsync(Url, content);

            var responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseBody);

            Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);

        }

        [Theory]
        [InlineData("-1")]
        [InlineData("invalid-ehr-id")]
        public async Task Post_CreateNewContributionWithInvalidEhrIdShouldreturnBadRequest(string ehrId)
        {

            Url = Url + "/" + ehrId + "/contribution";
            var content = Tests.GetTestContribution(_basePath);

            var response = await _client.PostAsync(Url, content);

            var responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseBody);

            Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        }

        [Fact]
        public async Task Post_CreateNewContributionWithNonExistingEhrIdShouldReturnNotFound()
        {
            var ehrId = Guid.NewGuid();

            Url = Url + "/" + ehrId + "/contribution";
            var content = Tests.GetTestContribution(_basePath);

            var response = await _client.PostAsync(Url, content);

            var responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseBody);

            Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);

        }




    }
}