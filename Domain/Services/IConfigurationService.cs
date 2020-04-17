namespace Domain.Services
{
    public interface IConfigurationService
    {
        T Get<T>(string sectionName) where T : new();
    }
}