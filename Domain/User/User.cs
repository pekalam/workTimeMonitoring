using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("DomainTestUtils")]
namespace Domain.User
{
    public class DomainPrimitive<TVal, TSubcl> : IEquatable<TSubcl>, IComparable<TSubcl>
        where TSubcl : DomainPrimitive<TVal, TSubcl> where TVal : IComparable<TVal>
    {
        public TVal Value { get; }

        public DomainPrimitive(TVal value)
        {
            Value = value;
        }

        public bool Equals(TSubcl other)
        {
            if (other == null)
                return false;
            return Value.Equals(other.Value);
        }

        public int CompareTo(TSubcl other)
        {
            if (other == null)
                return 1;
            return Value.CompareTo(other.Value);
        }

        public override bool Equals(object obj) => (obj is TSubcl) && Equals(obj as TSubcl);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
    }

    public class Username : DomainPrimitive<string, Username>
    {
        public const int MinLength = 3;
        private Regex _nameRegex = new Regex(@"^[a-zA-Z]+[0-9]*$");

        public Username(string value) : base(value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception();
            }

            if (value.Length < MinLength)
            {
                throw new Exception();
            }

            if (!_nameRegex.IsMatch(value))
            {
                throw new Exception();
            }
        }

        public static implicit operator string(Username username) => username.Value;
        public static implicit operator Username(string username) => new Username(username);
    }

    public class Password : DomainPrimitive<string, Password>
    {
        public const int MinLength = 3;

        public Password(string value) : base(value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception();
            }
        }

        public static implicit operator string(Password password) => password.Value;
        public static implicit operator Password(string password) => new Password(password);
    }


    public class User
    {
        public User(long userId, Username username)
        {
            UserId = userId;
            Username = username;
        }
        
        internal User()
        {

        }

        public long UserId { get; private set; }
        public Username Username { get; private set; }
    }

    public class AuthData
    {
        public AuthData(long userId, Password password)
        {
            UserId = userId;
            Password = password;
        }

        internal AuthData()
        {
            
        }

        public long UserId { get; private set; }
        public Password Password { get; private set; }
    }
}
