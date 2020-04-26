using Domain.User;
using Infrastructure;
using Prism.Commands;
using Prism.Regions;
using WindowUI.MainWindow;

namespace WindowUI.LoginWindow
{
    public interface ILoginViewController
    {
        DelegateCommand LoginCommand { get; set; }
        void Init(LoginViewModel vm);
    }

    public class LoginViewController : ILoginViewController
    {
        private LoginViewModel _vm;
        private readonly IAuthenticationService _authenticationService;
        private readonly IRegionManager _regionManager;

        public LoginViewController(IAuthenticationService authenticationService, IRegionManager regionManager)
        {
            _authenticationService = authenticationService;
            _regionManager = regionManager;
            LoginCommand = new DelegateCommand(LoginExecute, LoginCanExecute);
        }

        private bool LoginCanExecute()
        {
            if (_vm == null)
            {
                return false;
            }
            return !_vm.CheckHasErrors() && _vm.LoginDirty && _vm.PasswordDirty;
        }

        private void LoginExecute()
        {
            if (_authenticationService.Login(_vm.Login, _vm.PasswordValue) == null)
            {
                _vm.SetInvalidPassword();
            }
            else
            {
                _regionManager.Regions[ShellRegions.MainRegion].RequestNavigate(nameof(MainWindowView));
            }
        }

        public DelegateCommand LoginCommand { get; set; }

        public void Init(LoginViewModel vm)
        {
            _vm = vm;
            _authenticationService.Login("test", "pass");
            _regionManager.Regions[ShellRegions.MainRegion].RequestNavigate(nameof(MainWindowView));
        }
    }
}