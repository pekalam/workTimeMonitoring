using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using Domain.Repositories;
using Domain.User;
using FluentAssertions;
using Infrastructure.src.Repositories;
using Xunit;

namespace Infrastructure.Tests
{
    public abstract class UserRepositoryTests
    {
        private readonly IUserRepository _repository;

        protected UserRepositoryTests()
        {
            _repository = Create();
        }

        protected abstract IUserRepository Create();

        [Fact]
        public void Find_returns_valid_user()
        {
            var user = _repository.Find("test");

            user.Should().NotBeNull();
            user.UserId.Should().Be(1);
            user.Username.Value.Should().Be("test");
        }

        [Fact]
        public void Find_when_does_not_exists_returns_null()
        {
            var user = _repository.Find("notexisting");

            user.Should().BeNull();
        }
    }

    public class SqliteUserRepositoryTests : UserRepositoryTests
    {
        protected override IUserRepository Create()
        {
            return new SqliteUserRepository(TestUtils.ConfigurationService);
        }
    }

    public abstract class AuthenticationServiceTests
    {
        private readonly IAuthenticationService _authenticationService;

        protected abstract IAuthenticationService Create();

        protected AuthenticationServiceTests()
        {
            _authenticationService = Create();
        }

        [Fact]
        public void Login_when_user_exists_returns_user_and_updates_observalbe()
        {
            var sem = new SemaphoreSlim(0,1);
            var subs = _authenticationService.LoggedInUser.Subscribe(user =>
            {
                sem.Release();
                user.Should().BeNull();
            });

            sem.Wait(TimeSpan.FromSeconds(10));
            subs.Dispose();

            var logged = _authenticationService.Login("test", "pass");

            sem = new SemaphoreSlim(0,1);
            _authenticationService.LoggedInUser.Subscribe(user =>
            {
                sem.Release();
                user.Should().NotBeNull();
                user.Username.Value.Should().Be("test");
                user.UserId.Should().Be(1);
            });

            logged.Should().NotBeNull();
        }

        [Fact]
        public void Login_when_invalid_password_returns_null_and_updates_obserable()
        {
            int called = 0;
            var sem = new SemaphoreSlim(0, 2);
            _authenticationService.LoggedInUser.Subscribe(user =>
            {
                sem.Release();
                user.Should().BeNull();
                called++;
            });

            sem.Wait(TimeSpan.FromSeconds(10));

            var logged = _authenticationService.Login("test", "invalid");

            logged.Should().BeNull();

            sem.Wait(TimeSpan.FromSeconds(10));

            called.Should().Be(2);
        }

    }
}
