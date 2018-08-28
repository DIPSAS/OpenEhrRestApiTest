using System.Net.Http;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace OpenEhrRestApiTest
{
    public class OpenEhrRestApiTestFixture : IDisposable
    {
        public HttpClient Client { get; }
        public IConfiguration Configuration;
        public string Path {get;}

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
        }

        public void Dispose()
        {
            Client.Dispose();
        }


    }

    public class Tests { 
        public static void AddMandatoryOpenEhrRestApiHeaders(StringContent content){
            string committerName = "John Doe";
            string lifecycle_state = "532"; // 532 = complete
            AddMandatoryOpenEhrRestApiHeaders(content, committerName, lifecycle_state);
        }

        public static void AddMandatoryOpenEhrRestApiHeaders(StringContent content,  string committerName, string lifecycle_state){
            string preferRepresentation  = "representation";
            AddMandatoryOpenEhrRestApiHeaders(content, committerName, lifecycle_state, preferRepresentation);
        }
        public static void AddMandatoryOpenEhrRestApiHeaders(StringContent content, string committerName, string lifecycle_state, string preferRepresentation){
            var name = @"name="""+committerName+@"""";
            var state =  @"code_string="""+lifecycle_state+@""""; 
            var representation = @"return="""+preferRepresentation+@"""";
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Headers.Add("openEHR-AUDIT_DETAILS.committer", name);
            content.Headers.Add("openEHR-VERSION.lifecycle_state", state);
            content.Headers.Add("Prefer", representation);
        }
    }

}
