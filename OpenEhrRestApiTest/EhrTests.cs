using System;
using System.IO;
using Xunit;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DIPS.OpenEhr.RM.Common.GenericPackage;
using Microsoft.AspNetCore.Http;
using DIPS.OpenEhr.RM.Domain.Ehr.EhrPackage;
using DIPS.OpenEhr.RM.Support.IdentificationPackage;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit.Abstractions;
using System.Net.Http.Headers;

namespace OpenEhrRestApiTest
{
    public class EhrControllerTests : IClassFixture<OpenEhrRestApiTestFixture>
    {
        private const string Url = "ehr";
        private static HttpClient _client;
        private static string _path;

        private readonly ITestOutputHelper _output;

        public EhrControllerTests(OpenEhrRestApiTestFixture fixture, ITestOutputHelper output)
        {
            _client = fixture.Client;
            _path = fixture.Path;
            _output = output;
        }

        
        // Note that the below ID is found in the current test server, and will
        // not work on other servers. Future versions should ensure that the
        // test server is populated with test EHRs.
        [Theory]
        [InlineData("05fad39b-ecde-4bfe-92ad-cd1accc76a14")]
        public async Task Get_EhrWithValidEhrIdShouldReturnSuccess(string ehrId){
            var response = await _client.GetAsync(Url + "/" + ehrId);
            Assert.Equal(StatusCodes.Status200OK, (int) response.StatusCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData("100")]
        [InlineData("-1")]
        public async Task Get_EhrWithNonExistingEhrIdShouldReturnNotFound(string ehrId)
        {
            var response = await _client.GetAsync(Url + "/" + ehrId);
            Assert.Equal(StatusCodes.Status404NotFound, (int) response.StatusCode);
        }

        [Theory(Skip = "Not obvious as of 20.08.2018 in the REST API spec what should return a HTTP bad request.")]
        [InlineData("")]
        public async Task Get_EhrWithInvalidEhrIdShouldReturnBadRequest(string ehrId)
        {
            var response = await _client.GetAsync(Url + ehrId);
            Assert.Equal(StatusCodes.Status400BadRequest, (int) response.StatusCode);
        }

//        [Fact]
        public async Task Post_CreateEhrReturnsCreated()
        {
            var json = File.ReadAllText(Path.Combine(_path, "TestData/post-ehr.json")); 
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Headers.Add("openEHR-AUDIT_DETAILS.committer", "test-committer");
            content.Headers.Add("Prefer", "return=representation");
            
            var response = await _client.PostAsync(Url, content);
            _output.WriteLine(content.ToString());
            _output.WriteLine(response.ToString());
            Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
         }
    }
}