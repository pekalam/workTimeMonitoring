using Domain.User;
using Infrastructure.src.Repositories;
using Infrastructure.src.Services;

namespace Infrastructure.Tests
{
    public class SqliteAuthenticationServiceTests : AuthenticationServiceTests
    {
        protected override IAuthenticationService Create()
        {
            return new AuthenticationService(new SqliteAuthDataRepository(TestUtils.ConfigurationService), new SqliteUserRepository(TestUtils.ConfigurationService));
        }
    }
}