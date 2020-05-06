using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Infrastructure.Messaging;
using Prism.Events;
using Prism.Mvvm;

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
