using System;
using System.Collections.Generic;
using System.Text;
using Domain.User;

namespace Domain.Repositories
{
    public interface IUserRepository
    {
        User.User? Find(Username username);
    }

    public interface IAuthDataRepository
    {
        AuthData Find(long userId);
        void Add(AuthData authData);
    }
}
