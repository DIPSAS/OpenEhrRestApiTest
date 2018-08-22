using System.Net.Http;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace OpenEhrRestApiTest
{
    public class OpenEhrRestApiTestFixture : IDisposable
    {
        public HttpClient Client { get; }
        public IConfiguration Configuration;

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

            Client.BaseAddress = new Uri(protocol + "://" + hostname + ":" + port + "/");
            Console.WriteLine(Client);
        }

        public void Dispose()
        {
            Client.Dispose();
        }
    }

}


// Endpoints 
// - Create EHR, /api/v1/ehr/ --> http post /ehr (not put)
// - Get Version /api/v1/server/version --> ???
// - Load Template Ids /api/v1/template ---> http get /template
// - Import template /api/v1/template ---> http post /template
// - Load template /api/v1/template/{template_id}/opt ---> get /template/{template_id}
// - Save composition /api/v1/contribution
// - Commit contriution /api/v1/{ehr_id}/contribution?committerName=.. --> POST {ehr_id}/contribution
// - RunAqlAsync /avpi/v1/query... --> POST /aql/ ... 
// - ValidateAqlAsync /api/v1/query/validate ... --> (not in the ehr rest spec)