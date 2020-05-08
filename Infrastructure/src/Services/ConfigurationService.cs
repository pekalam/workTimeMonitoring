using Domain.Services;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;

namespace Infrastructure.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfigurationRoot? _root;
        private Dictionary<string, Func<object>> _customFactories = new Dictionary<string, Func<object>>();

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

        public T Get<T>(string sectionName) where T : class, new()
        {
            if (_customFactories.ContainsKey(sectionName))
            {
                if (!(_customFactories[sectionName]() is T inst))
                {
                    throw new Exception("Invalid custom type");
                }
                return inst;
            }
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

        public void RegisterCustomInstance<T>(string sectionName, Func<T> factory) where T : class
        {
            _customFactories[sectionName] = factory;
        }
    }
}
