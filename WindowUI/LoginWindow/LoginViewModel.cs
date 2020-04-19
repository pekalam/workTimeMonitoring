using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Domain.User;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace WindowUI.LoginWindow
{
    public class LoginViewModel : BindableBase, INavigationAware, INotifyDataErrorInfo
    {
        private string _password;
        private string _login;
        private readonly ILoginViewController _controller;
        private bool _invalidPassword;
        private bool _passwordDirty = false;
        private bool _loginDirty = false;


        public LoginViewModel(ILoginViewController controller)
        {
            _controller = controller;
            LoginCommand = controller.LoginCommand;
        }

        public DelegateCommand LoginCommand { get; }

        public string Login
        {
            get => _login;
            set
            {
                SetProperty(ref _login, value);
                _loginDirty = value?.Length > 0;
                InvalidPassword = false;
                LoginCommand.RaiseCanExecuteChanged();
            }
        }

        public bool PasswordDirty => _passwordDirty;
        public bool LoginDirty => _loginDirty;

        public bool InvalidPassword
        {
            get => _invalidPassword;
            set => SetProperty(ref _invalidPassword, value);
        }

        internal string PasswordValue { get; set; } = "";

        public string Password
        {
            get => _password?.Select(s => "*").Aggregate("", (a, b) => a + b);
            set
            {
                SetProperty(ref _password, value);
                _passwordDirty = value?.Length > 0;
                InvalidPassword = false;
                LoginCommand.RaiseCanExecuteChanged();
            }
        }

        public void SetInvalidPassword()
        {
            InvalidPassword = true;
        }

        private string[] ValidatePassword()
        {
            List<string> errors = new List<string>();

            if (_loginDirty)
            {
                if (Password?.Length < Domain.User.Password.MinLength)
                {
                    errors.Add("Invalid password length");
                }

                if (string.IsNullOrWhiteSpace(Password))
                {
                    errors.Add("Password cannot be empty");
                }
            }


            return errors.ToArray();
        }

        private string[] ValidateLogin()
        {
            List<string> errors = new List<string>();

            if (_loginDirty)
            {
                if (Login?.Length < Username.MinLength)
                {
                    errors.Add("Invalid login length");
                }

                if (string.IsNullOrWhiteSpace(Login))
                {
                    errors.Add("Login cannot be empty");
                }
            }


            return errors.ToArray();
        }

        public bool CheckHasErrors() => ValidateLogin().Length > 0 || ValidatePassword().Length > 0 || _invalidPassword;

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _controller.Init(this);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public IEnumerable GetErrors(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(Password):
                    return ValidatePassword();
                case nameof(Login):
                    return ValidateLogin();
                default:
                    return null;
            }
        }


        public bool HasErrors => CheckHasErrors();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
    }
}