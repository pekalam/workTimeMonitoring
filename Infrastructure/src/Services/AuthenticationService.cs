﻿using Domain.Repositories;
using Domain.User;
using System;
using System.Reactive.Subjects;

namespace Infrastructure.src.Services
{
    internal class AuthenticationService : IAuthenticationService
    {
        private readonly BehaviorSubject<User?> _loggedInUser = new BehaviorSubject<User?>(null);
        private User? _loggedIn;
        private readonly IAuthDataRepository _authDataRepository;
        private readonly IUserRepository _userRepository;

        public AuthenticationService(IAuthDataRepository authDataRepository, IUserRepository userRepository)
        {
            _authDataRepository = authDataRepository;
            _userRepository = userRepository;
        }

        private void SetUser(User? user)
        {
            _loggedIn = user;
            _loggedInUser.OnNext(user);
        }

        public User? Login(Username username, Password password)
        {
            var user = _userRepository.Find(username);
            if (user == null)
            {
                SetUser(null);
                return null;
            }

            var authData = _authDataRepository.Find(user.UserId);

            if (authData.Password.Equals(password))
            {
                SetUser(user);
                return user;
            }

            SetUser(null);
            return null;
        }

        public IObservable<User?> LoggedInUser => _loggedInUser;
        public User? User => _loggedIn;
    }
}
