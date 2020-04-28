using System;

namespace Domain.Services
{
    public interface IConfigurationService
    {
        T Get<T>(string sectionName) where T : class, new();
        void RegisterCustomInstance<T>(string sectionName, Func<T> factory) where T : class;
    }
}