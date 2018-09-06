using System.Net.Http;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using Xunit;
using Xunit.Sdk;
using Xunit.Abstractions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

using  Newtonsoft.Json.Linq;

namespace OpenEhrRestApiTest
{
    public class OpenEhrRestApiTestFixture : IDisposable
    {
        public HttpClient Client { get; }
        public IConfiguration Configuration;
        public string Path { get; }

        public string TestEhrId { get; }

        public OpenEhrRestApiTestFixture()
        {
            Client = new HttpClient();
            var configFilename = "settings.json";
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configFilename, optional: false, reloadOnChange: true);
            Configuration = builder.Build();

            var hostname = Configuration["ServerHostname"];
            var port = Configuration["ServerPort"];
            var protocol = Configuration["Protocol"];

            if (hostname == null || port == null || protocol == null)
            {
                throw new Exception("Configuration file `" + configFilename + "` is missing hostname, port or protocol.");
            }

            var baseUrl = "/openehr/latest/";

            Client.BaseAddress = new Uri(protocol + "://" + hostname + ":" +
                port + baseUrl);

            Path = AppDomain.CurrentDomain.BaseDirectory;

            TestEhrId = Tests.CreateTestEhr(Client, Path);

        }

        public void Dispose()
        {
            Client.Dispose();
        }
    }

    public class Tests
    {
        public static string CreateTestEhr(HttpClient client, string basePath)
        {
            var content = GetTestEhrPostContent(basePath);
            var url = "ehr"; 
            var response = client.PostAsync(url, content);

            var responseBody = Task.Run(() => response.Result.Content.ReadAsStringAsync()).Result;
            if ((int)response.Result.StatusCode != StatusCodes.Status201Created)
            {
                throw new Exception("Could not create new test EHR: HTTP" + response.Result.StatusCode.ToString() + " " + responseBody);
            } else { 
                JObject ehr = JObject.Parse(responseBody);
                var ehrId = (string) ehr["ehr_id"];
                return ehrId;
            }
        }

        public async static Task<string> CreateTestComposition(HttpClient client, string basePath, string ehrId)
        {
            var Url = "ehr/" + ehrId + "/composition";
            var content = GetTestEhrComposition(basePath);
            var response =  await client.PostAsync(Url, content);

            // Get ETag header = composition version id
            IEnumerable<string> etagheader = response.Headers.GetValues("ETag");
            var e= etagheader.GetEnumerator();
            e.MoveNext();
            return e.Current;
        }

        public static StringContent GetTestEhrComposition(string basePath){
            var testEhrCompositionFilename = Path.Combine(basePath, "TestData/example-composition.json");
            var json = System.IO.File.ReadAllText(testEhrCompositionFilename);

            JObject composition = JObject.Parse(json);
            var objectId = Guid.NewGuid();
            var creatingSystemId = "example.domain.com";
            var versionTreeId = "1";
            composition["uid"]["value"] = objectId.ToString() + "::" +creatingSystemId+"::" + versionTreeId;

            var content = new StringContent(composition.ToString(), Encoding.UTF8, "application/json");
            AddMandatoryOpenEhrRestApiHeaders(content);
            return content; 

        }

        public static StringContent GetTestEhrPostContent(string basePath){
            var testEhrStatusFilename = Path.Combine(basePath, "TestData/post-ehr.json");
            var json = File.ReadAllText(testEhrStatusFilename);

            JObject ehrStatus = JObject.Parse(json);
            ehrStatus["subject"]["external_ref"]["id"]["value"] = RandomString(5);

            var content = new StringContent(ehrStatus.ToString(), Encoding.UTF8, "application/json");
            AddMandatoryOpenEhrRestApiHeaders(content);
            return content;
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            StringBuilder builder = new StringBuilder();
            char c;
            for (int i = 0; i < length; i++)
            {
                c = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(c);
            }
            return builder.ToString();
        }

        public static void AddMandatoryOpenEhrRestApiHeaders(StringContent content)
        {
            string committerName = "John Doe";
            string lifecycle_state = "532"; // 532 = complete
            AddMandatoryOpenEhrRestApiHeaders(content, committerName, lifecycle_state);
        }

        public static void AddMandatoryOpenEhrRestApiHeaders(StringContent content, string committerName, string lifecycle_state)
        {
            string preferRepresentation = "representation";
            AddMandatoryOpenEhrRestApiHeaders(content, committerName, lifecycle_state, preferRepresentation);
        }
        public static void AddMandatoryOpenEhrRestApiHeaders(StringContent content, string committerName, string lifecycle_state, string preferRepresentation)
        {
            var name = @"name=""" + committerName + @"""";
            var state = @"code_string=""" + lifecycle_state + @"""";
            var representation = @"return=""" + preferRepresentation + @"""";
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Headers.Add("openEHR-AUDIT_DETAILS.committer", name);
            content.Headers.Add("openEHR-VERSION.lifecycle_state", state);
            content.Headers.Add("Prefer", representation);
        }
    }
}