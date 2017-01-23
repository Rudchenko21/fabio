using System;
using Microsoft.Extensions.Configuration;
using Nanophone.AspNetCore.ConfigurationProvider;
using Nanophone.Core;

namespace WebApplication1
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddNanophoneKeyValues(this IConfigurationBuilder builder, Func<IRegistryHost> registryHostFactory)
        {
            builder.Add(new NanophoneConfigurationSource(registryHostFactory));
            return builder;
        }
    }
}
