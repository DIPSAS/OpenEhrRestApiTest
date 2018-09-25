using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OpenEhrRestApiTest
{
    public class DirectoryTests : IClassFixture<OpenEhrRestApiTestFixture>
    {
        private string Url;
        private readonly HttpClient _client;
        private string _testEhrId;

        private readonly ITestOutputHelper _output;

        public DirectoryTests(OpenEhrRestApiTestFixture fixture, ITestOutputHelper output)
        {
            _client = fixture.Client;
            _output = output;
            _testEhrId = fixture.TestEhrId;
            Url = $"ehr/{_testEhrId}/directory";
        }

        [Fact]

        public async Task Post_CreateNewEmptyDirectoryShouldReturnSuccess()
        {
            var folder = new JObject();
            folder["_type"] = "FOLDER";
            folder["items"] = new JArray();
            folder["folders"] = new JArray();

            var content = new StringContent(folder.ToString());

            Tests.AddMandatoryOpenEhrRestApiHeaders(content);

            var response = await _client.PostAsync(Url, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseBody);

            Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);
        }

    }
}