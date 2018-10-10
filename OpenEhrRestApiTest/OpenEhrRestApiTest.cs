using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

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

            var version = "v1.0";
            var baseUrl = $"/openehr/{version}/";

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
            }
            else
            {
                JObject ehr = JObject.Parse(responseBody);
                var ehrId = (string)ehr["ehr_id"]["value"];
                return ehrId;
            }
        }

        public async static Task<string> CreateDummyFolder(HttpClient client, string ehrId)
        {
            var content = Tests.CreateEmptyFolderRequest();
            var url = $"ehr/{ehrId}/directory";
            var response = client.PostAsync(url, content);
            var responseBody = Task.Run(() => response.Result.Content.ReadAsStringAsync()).Result;

            if ((int)response.Result.StatusCode != StatusCodes.Status201Created)
            {
                throw new Exception($"Could not create test folder: HTTP{response.Result.StatusCode}: {responseBody}");
            }
            else
            {
                JObject folder = JObject.Parse(responseBody);
                return (string)folder["id"]["value"];
            }
        }

        public async static Task<string> CreateTestComposition(HttpClient client, string basePath, string ehrId)
        {
            var Url = "ehr/" + ehrId + "/composition";
            var content = GetTestComposition(basePath);
            var response = await client.PostAsync(Url, content);

            // Get ETag header = composition version id
            IEnumerable<string> etagheader = response.Headers.GetValues("ETag");
            var e = etagheader.GetEnumerator();
            e.MoveNext();
            return e.Current;
        }

        public static StringContent GetTestContribution(string basePath)
        {
            var contribution = CreateTestContribution(basePath);
            var content = new StringContent(contribution.ToString(), Encoding.UTF8, "application/json");
            AddMandatoryOpenEhrRestApiHeaders(content);
            return content;
        }

        public static StringContent GetInvalidTestContribution(string basePath)
        {
            var contribution = CreateTestContribution(basePath);

            // Sets the changetype of a new contribution from creation to
            // modification. 
            contribution["audit"]["change_type"]["defining_code"]["code_string"] = 251;
            contribution["audit"]["change_type"]["value"] = "modification";

            var content = new StringContent(contribution.ToString(), Encoding.UTF8, "application/json");
            AddMandatoryOpenEhrRestApiHeaders(content);
            return content;
        }

        public static StringContent CreateEmptyFolderRequest()
        {
            var folder = CreateEmptyFolder();
            return CreateFolderRequest(folder);
        }
        public static StringContent CreateFolderRequest(JObject folder)
        {
            var content = new StringContent(folder.ToString());
            AddMandatoryOpenEhrRestApiHeaders(content);
            return content;
        }

        public static JObject CreateEmptyFolder()
        {
            var folder = new JObject();
            folder["_type"] = "FOLDER";
            folder["items"] = new JArray();
            folder["folders"] = new JArray();
            return folder;
        }


        public static JObject CreateTestAqlQuery(int fetch, int offset)
        {
            var aql = "SELECT c FROM COMPOSITION c limit 1";
            return CreateTestAqlQuery(aql, fetch, offset);
        }

        public static JObject CreateTestAqlQuery(string aql, int fetch, int offset)
        {
            JObject query = new JObject();
            query["q"] = aql;
            query["offset"] = offset;
            query["fetch"] = fetch;
            return query;
        }

        public static JObject CreateTestAqlQuery(string aql, Dictionary<string, Object> queryParameters, int fetch, int offset)
        {
            var query = CreateTestAqlQuery(aql, fetch, offset);
            query["query_parameters"] = JObject.FromObject(queryParameters);
            return query;
        }

        private static JObject CreateTestContribution(string basePath)
        {
            var testContributionFilename = Path.Combine(basePath, "TestData/example-contribution.json");
            var json = File.ReadAllText(testContributionFilename);
            JObject contribution = JObject.Parse(json);

            JObject composition = getTestCompositionObject(basePath);

            JObject versionObject = new JObject();

            string compositionNamespace = (string)composition["composer"]["namespace"];
            versionObject["namespace"] = compositionNamespace;

            versionObject["type"] = "COMPOSITION";
            versionObject["data"] = composition;

            var versions = new JArray();
            versions.Add(versionObject);

            contribution["versions"] = versions;
            return contribution;
        }

        public static StringContent GetTestComposition(string basePath)
        {
            var composition = getTestCompositionObject(basePath);
            var content = new StringContent(composition.ToString(), Encoding.UTF8, "application/json");
            AddMandatoryOpenEhrRestApiHeaders(content);
            return content;
        }

        private static JObject getTestCompositionObject(string basePath)
        {
            var testCompositionFilename = Path.Combine(basePath, "TestData/example-composition.json");
            var json = File.ReadAllText(testCompositionFilename);
            var composition = parseComposition(json);
            return composition;
        }

        private static JObject parseComposition(string json)
        {
            JObject composition = JObject.Parse(json);
            var objectId = Guid.NewGuid();
            var creatingSystemId = "example.domain.com";
            var versionTreeId = "1";
            composition["uid"]["value"] = objectId.ToString() + "::" + creatingSystemId + "::" + versionTreeId;
            return composition;
        }

        public static StringContent GetTestEhrPostContent(string basePath)
        {
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

        public static StringContent GetTestTemplate(string basePath)
        {
            var template = ReadTestTemplate(basePath);
            return CreateTemplateContent(template);
        }

        // Returns an invalid template, i.e. the first half of a functioning opt file.
        public static StringContent GetInvalidTestTemplate(string basePath)
        {
            var template = ReadTestTemplate(basePath);
            template = template.Substring(template.Length / 2);
            return CreateTemplateContent(template);
        }

        private static string ReadTestTemplate(string basePath)
        {
            var testTemplateFilename = Path.Combine(basePath, "TestData/example-bp-temp-weight-template.opt");
            var template = File.ReadAllText(testTemplateFilename);
            return template;
        }

        private static StringContent CreateTemplateContent(string template)
        {
            StringContent content = new StringContent(template);
            AddMandatoryOpenEhrRestApiHeaders(content);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            return content;
        }

        public static async Task<string> CreateTestTemplate(HttpClient client, string Url, string basePath)
        {
            var content = Tests.GetTestTemplate(basePath);
            client.DefaultRequestHeaders.Add("Accept", "application/xml");
            var response = await client.PostAsync(Url, content);
            client.DefaultRequestHeaders.Remove("Accept");

            var responseBody = await response.Content.ReadAsStringAsync();

            if ((int)response.StatusCode != StatusCodes.Status201Created)
            {
                throw new Exception("Could not create test template");
            }

            IEnumerable<string> etagheader = response.Headers.GetValues("Location");
            var e = etagheader.GetEnumerator();
            e.MoveNext();
            return e.Current;
        }
    }
}