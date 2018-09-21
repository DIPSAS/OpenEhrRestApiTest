using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace OpenEhrRestApiTest
{
    public class OptionsControllerTests : IClassFixture<OpenEhrRestApiTestFixture>
    {
        private const string Url = "";
        private static HttpClient _client;
        private readonly ITestOutputHelper _output;

        public OptionsControllerTests(OpenEhrRestApiTestFixture fixture, ITestOutputHelper output)
        {
            _client = fixture.Client;
            _output = output;
        }


        // Note that the below ID is found in the current test server, and will
        // not work on other servers. Future versions should ensure that the
        // test server is populated with test EHRs.
        [Fact]
        public async Task Get_EhrWithValidEhrIdShouldReturnOk()
        {
            var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod("OPTIONS"), Url));

            var responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseBody);
            JObject options = JObject.Parse(responseBody);

            Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
            Assert.NotNull(options["solution"]);
            Assert.NotNull(options["solution_version"]);
            Assert.NotNull(options["vendor"]);
            Assert.NotNull(options["rest_api_specs_version"]);
            Assert.NotNull(options["conformance_profile"]);
            Assert.NotNull(options["endpoints"]);
        }


    }
}
