using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nanophone.Core;
using Nanophone.RegistryHost.ConsulRegistry;
using Nanophone.AspNetCore.ApplicationServices;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                 .SetBasePath(env.ContentRootPath)
                 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                 .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(Configuration);
            var appSettings = new AppSettings();
            Configuration.Bind(appSettings);
            var consulConfig = new ConsulRegistryHostConfiguration { HostName = appSettings.Consul.HostName, Port = appSettings.Consul.Port };
            services.AddNanophone(() => new ConsulRegistryHost(consulConfig));
            services.AddMvc();
            services.AddOptions();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            var localAddress = DnsHelper.GetIpAddressAsync().Result;
            var uri = new Uri($"http://{localAddress}:{Program.Port}/");
            var registryInformation = app.AddTenant("1212", "1.7.0-pre", uri, healthCheckUri: new Uri($"http://{localAddress}:{Program.Port}/api/values"), tags: new[] { "urlprefix-/values" });
            var checkId = app.AddHealthCheck(registryInformation, new Uri(uri, "randomvalue"), TimeSpan.FromSeconds(15), "random value");

            app.ApplicationServices.GetService<IOptions<HealthCheckOptions>>().Value.HealthCheckId = checkId;

            applicationLifetime.ApplicationStopping.Register(() =>
            {
                app.RemoveHealthCheck(checkId);
                app.RemoveTenant(registryInformation.Id);
            });
        }
    }
}
