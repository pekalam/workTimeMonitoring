using Prism.Events;
using Prism.Mvvm;
using System.Windows.Controls;
using UI.Common.Messaging;

namespace WindowUI.SplashScreen
{
    /// <summary>
    /// Interaction logic for SplashScreenView.xaml
    /// </summary>
    public partial class SplashScreenView : UserControl
    {
        public SplashScreenView()
        {
            InitializeComponent();
        }
    }

    public class SplashScreenViewModel : BindableBase
    {
        private IEventAggregator _ea;
        private string _message;

        public SplashScreenViewModel(IEventAggregator ea)
        {
            _ea = ea;
            _ea.GetEvent<SplashScreenMsgEvent>().Subscribe(msg => Message = msg, true);
        }


        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }
    }
}
