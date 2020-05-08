using Prism.Mvvm;
using System.Windows.Controls;

namespace WindowUI.Statistics
{
    /// <summary>
    /// Interaction logic for SeriesPickerView.xaml
    /// </summary>
    public partial class SeriesPickerView : UserControl
    {
        public SeriesPickerView()
        {
            InitializeComponent();
        }
    }

    public class SeriesPickerViewModel : BindableBase
    {
        private bool _showMouse = true;
        private bool _showWatchingScreen = true;
        private bool _showKeyboard = true;
        private bool _showAway = true;

        public bool ShowMouse
        {
            get => _showMouse;
            set => SetProperty(ref _showMouse, value);
        }

        public bool ShowWatchingScreen
        {
            get => _showWatchingScreen;
            set => SetProperty(ref _showWatchingScreen, value);
        }

        public bool ShowKeyboard
        {
            get => _showKeyboard;
            set => SetProperty(ref _showKeyboard, value);
        }

        public bool ShowAway
        {
            get => _showAway;
            set => SetProperty(ref _showAway, value);
        }
    }
}
