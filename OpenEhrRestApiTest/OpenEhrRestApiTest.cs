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

}
