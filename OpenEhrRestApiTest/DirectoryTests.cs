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
            var content = Tests.CreateEmptyFolderRequest();
            var response = await _client.PostAsync(Url, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseBody);

            Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);
        }

        [Fact]
        public async Task Post_CreateNewInvalidDirectoryShouldReturnBadRequest()
        {
            var folder = Tests.CreateEmptyFolder();
            folder["items"] = "items should be an array, not a string";
            var content = Tests.CreateFolderRequest(folder);
            var response = await _client.PostAsync(Url, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseBody);
            Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        }

        [Fact]
        public async Task Post_CreateNewEmptyDirectoryWithInvalidEhrIdShouldReturnNotFound()
        {
            var ehrId = Guid.NewGuid();
            Url = "ehr/" + ehrId + "/directory";
            var content = Tests.CreateEmptyFolderRequest();
            var response = await _client.PostAsync(Url, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseBody);

            Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
        }
    }
}