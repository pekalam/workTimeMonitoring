using AutoMapper;

namespace Infrastructure.Db
{
    internal static class MapperConfigFactory
    {
        public static MapperConfiguration Create()
        {
            return new MapperConfiguration(cfg => cfg.AddProfile<DbTestImageProfile>());
        }
    }
}