using System.Windows.Controls;
using System.Windows.Input;
using Serilog;

namespace WindowUI.LoginWindow
{
    /// <summary>
    /// Interaction logic for LoginView
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                var vm = (DataContext as LoginViewModel);
                vm.PasswordValue += " ";
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var vm = (DataContext as LoginViewModel);
            vm.PasswordValue = vm.PasswordValue.Substring(0, password.Text.Length);
        }

        private void password_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var vm = (DataContext as LoginViewModel);
            vm.PasswordValue += e.Text;
        }
    }
}
