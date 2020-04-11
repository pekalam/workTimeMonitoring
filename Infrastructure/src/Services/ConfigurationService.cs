using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Infrastructure.Services
{
    public class ConfigurationService
    {
        private readonly IConfigurationRoot? _root;

        public ConfigurationService(string configurationFilePath)
        {
            if (!File.Exists(configurationFilePath))
            {
                _root = null;
            }
            else
            {
                _root = new ConfigurationBuilder().AddJsonFile(configurationFilePath).Build();
            }

        }

        public T Get<T>(string sectionName) where T : new()
        {
            if (_root != null)
            {
                var config = _root.GetSection(sectionName).Get<T>();
                if (config == null)
                {
                    throw new NullReferenceException($"Cannot find settings for section: {sectionName}");
                }

                return config;
            }
            else
            {
                return new T();
            }
        }
    }
}
