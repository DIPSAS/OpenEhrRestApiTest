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
            Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);
            Assert.True(response.Headers.Contains("Location"), "Response header must contain Location.");
            Assert.True(response.Headers.Contains("ETag"), "Response header must contain ETag.");
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
            Url = $"ehr/{ehrId}/directory";
            var content = Tests.CreateEmptyFolderRequest();
            var response = await _client.PostAsync(Url, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseBody);

            Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
        }

        [Fact]
        public async Task Get_ExistingFolderShouldReturnSuccess()
        {
            var folderUid = await Tests.CreateDummyFolder(_client, _testEhrId);
            var Url = $"ehr/{_testEhrId}/directory/{folderUid}";

            var response = await _client.GetAsync(Url);

            Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        }

        [Fact]
        public async Task Get_NonexistingFolderShouldReturnNotFound()
        {
            var folderUid = Guid.NewGuid();
            var Url = $"ehr/{_testEhrId}/directory/{folderUid}";
            var response = await _client.GetAsync(Url);

            Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
        }


        [Fact]
        public async Task Get_FolderAtNonexistingPathShouldReturnNoContent()
        {
            var folderUid = await Tests.CreateDummyFolder(_client, _testEhrId);
            var Url = $"ehr/{_testEhrId}/directory/{folderUid}?path=nonExistingPath/";
            var response = await _client.GetAsync(Url);

            Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
        }

        [Fact]
        public async Task Get_FolderForNonExistingEhrIdShouldReturnNotFound()
        {
            var folderUid = await Tests.CreateDummyFolder(_client, _testEhrId);
            var nonExistingEhrId = Guid.NewGuid();
            var Url = $"ehr/{nonExistingEhrId}/directory/{folderUid}";
            var response = await _client.GetAsync(Url);

            Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
        }



    }
}