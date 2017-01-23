using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Nanophone.RegistryHost.ConsulRegistry;
using Newtonsoft.Json;

namespace WebApplication1
{
    public class Program
    {
        public const int Port = 9030;

        public static void Main(string[] args)
        {
            var appsettings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText("appsettings.json"));
            var consulConfig = new ConsulRegistryHostConfiguration { HostName = appsettings.Consul.HostName, Port = appsettings.Consul.Port };
            var config = new ConfigurationBuilder()
                .AddNanophoneKeyValues(() => new ConsulRegistryHost(consulConfig))
                .Build();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://*:{Port}")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseConfiguration(config)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
