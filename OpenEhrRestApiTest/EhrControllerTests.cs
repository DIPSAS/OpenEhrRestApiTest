using System;
using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OpenEhrRestApiTest
{
    public class EhrControllerTests : IClassFixture<OpenEhrRestApiTestFixture>
    {
        private const string Url = "api/openehr/latest/ehr/";
        private static HttpClient _client;

        public EhrControllerTests(OpenEhrRestApiTestFixture fixture)
        {
            _client = fixture.Client;
        }

        //[Theory]
        //[InlineData("api/v1/ehr/")]
        //public async Task Get_EhrOriginalApiReturnsSuccess(string url)
        //{
        //    var response = await _client.GetAsync(url);
        //    Console.WriteLine(response);
        //    response.EnsureSuccessStatusCode();
        //    Assert.Equal(StatusCodes.Status200OK, (int) response.StatusCode);
        //}

        [Theory]
        [InlineData("")]
        [InlineData("100")]
        [InlineData("-1")]
        public async Task Get_EhrWithNonExistingEhrIdShouldReturnNotFound(string ehrId)
        {
            var response = await _client.GetAsync(Url + ehrId);
            Assert.Equal(StatusCodes.Status404NotFound, (int) response.StatusCode);
        }

        [Theory(Skip = "Not obvious as of 20.08.2018 in the REST API spec what should return a HTTP bad request.")]
        [InlineData("")]
        public async Task Get_EhrWithInvalidEhrIdShouldReturnBadRequest(string ehrId)
        {
            var response = await _client.GetAsync(Url + ehrId);
            Assert.Equal(StatusCodes.Status400BadRequest, (int) response.StatusCode);


        }
    }
}