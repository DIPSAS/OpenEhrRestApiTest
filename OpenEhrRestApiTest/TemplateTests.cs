using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OpenEhrRestApiTest
{
    public class TemplateTests : IClassFixture<OpenEhrRestApiTestFixture>
    {
        private string Url;
        private readonly HttpClient _client;
        private string _testEhrId;
        private string _basePath;

        private readonly ITestOutputHelper _output;

        public TemplateTests(OpenEhrRestApiTestFixture fixture, ITestOutputHelper output)
        {
            _client = fixture.Client;
            _output = output;
            _testEhrId = fixture.TestEhrId;
            _basePath = fixture.Path;
            Url = "definition/template";
        }

        [Fact]
        public async Task Post_CreateNewTemplateShouldReturnSuccess()
        {
            Url = Url + "?type=opt14";

            var content = Tests.GetTestTemplate(_basePath);
            _client.DefaultRequestHeaders.Add("Accept", "application/xml");

            var response = await _client.PostAsync(Url, content);

            var responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseBody);

            Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);
            Assert.True(response.Headers.Contains("Location"), "Response header must contain Location.");
        }

        [Fact]
        public async Task Post_CreateNewTemplateWithInvalidContentShouldReturnBadRequest()
        {

            Url = Url + "?type=opt14";

            var content = Tests.GetInvalidTestTemplate(_basePath);
            var response = await _client.PostAsync(Url, content);

            var responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseBody);

            Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        }

        [Theory]
        [InlineData("application/xml")]
        [InlineData("application/json")]
        public async Task Get_ExistingTemplateShouldReturnSuccess(string format)
        {
            Url = Url + "?type=opt14";
            var templateUrl = await Tests.CreateTestTemplate(_client, Url, _basePath);

            _client.DefaultRequestHeaders.Add("Accept", format);
            var response = await _client.GetAsync(templateUrl);
            _client.DefaultRequestHeaders.Remove("Accept");

            var responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseBody);

            Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        }

        [Fact]
        public async Task Get_NonExistingTemplateShouldReturnNotFound()
        {
            var invalidTemplateId = Guid.NewGuid();
            Url = Url + $"/{invalidTemplateId.ToString()}";

            var response = await _client.GetAsync(Url);
            var responseBody = await response.Content.ReadAsStringAsync();

            Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
        }

        [Fact]
        public async Task Get_AllTemplatesShouldReturnSuccess()
        {
            Url = Url + "?type=opt14";
            await Tests.CreateTestTemplate(_client, Url, _basePath);
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            var response = await _client.GetAsync(Url);
            _client.DefaultRequestHeaders.Remove("Accept");

            var responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseBody);

            JArray templates = JArray.Parse(responseBody);

            Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

            // Check first template info for necessary fields
            Assert.NotNull(templates[0]["type"]);
            Assert.NotNull(templates[0]["template_id"]);
            Assert.NotNull(templates[0]["version"]);
            Assert.NotNull(templates[0]["concept"]);
            Assert.NotNull(templates[0]["archetype_id"]);
            Assert.NotNull(templates[0]["created_timestamp"]);

        }
    }
}
