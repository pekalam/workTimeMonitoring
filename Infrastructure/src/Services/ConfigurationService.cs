using System;
using System.Diagnostics;
using System.IO;
using Domain;
using Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Serilog;

namespace Infrastructure.Services
{
    public class ConfigurationService : IConfigurationService
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
                    Log.Logger.Information($"Cannot find settings for section: {sectionName}");
                    return new T();
                }

                return config;
            }

            return new T();
        }
    }
}
