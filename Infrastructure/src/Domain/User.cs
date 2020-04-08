using System;
using System.Text.RegularExpressions;

namespace Infrastructure.Domain
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

            if (value.Length < MinLength)
            {
                throw new Exception();
            }
        }
    }

    public class User
    {
        public User(Username username, Password password, bool isAdmin)
        {
            Username = username;
            Password = password;
            IsAdmin = isAdmin;
        }

        public Username Username { get; }
        public Password Password { get; }
        public bool IsAdmin { get; }
    }
}
