using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.User
{
    public interface IAuthenticationService
    {
        User? Login(Username username, Password password);
        IObservable<User?> LoggedInUser { get; }
        User? User { get; }
    }
}
