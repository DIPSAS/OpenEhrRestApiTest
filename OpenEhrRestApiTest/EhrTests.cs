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

            if((int) response.StatusCode != StatusCodes.Status200OK) { 
                var responseBody =   await response.Content.ReadAsStringAsync();
                _output.WriteLine(responseBody);
            }
            
            Assert.Equal(StatusCodes.Status200OK, (int) response.StatusCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData("100")]
        [InlineData("-1")]
        public async Task Get_EhrWithNonExistingEhrIdShouldReturnNotFound(string ehrId)
        {
            var response = await _client.GetAsync(Url + "/" + ehrId);

            if((int) response.StatusCode != StatusCodes.Status404NotFound) { 
                var responseBody =   await response.Content.ReadAsStringAsync();
                _output.WriteLine(responseBody);
            }
            
            Assert.Equal(StatusCodes.Status404NotFound, (int) response.StatusCode);
        }

        [Theory(Skip = "Not obvious as of 20.08.2018 in the REST API spec what should return a HTTP bad request.")]
        [InlineData("")]
        public async Task Get_EhrWithInvalidEhrIdShouldReturnBadRequest(string ehrId)
        {
            var response = await _client.GetAsync(Url + ehrId);

            if((int) response.StatusCode != StatusCodes.Status400BadRequest) { 
                var responseBody =   await response.Content.ReadAsStringAsync();
                _output.WriteLine(responseBody);
            }
            
            Assert.Equal(StatusCodes.Status400BadRequest, (int) response.StatusCode);
        }

        [Fact]
        public async Task Post_CreateNewEhrReturnsCreated()
        {
            var json = File.ReadAllText(Path.Combine(_path, "TestData/post-ehr.json")); 
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            Tests.AddMandatoryOpenEhrRestApiHeaders(content);
            
            var response = await _client.PostAsync(Url, content);
            if((int) response.StatusCode != StatusCodes.Status201Created) { 
                var responseBody =   await response.Content.ReadAsStringAsync();
                _output.WriteLine(responseBody);
            }

            Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);
         }
    }
}