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

        private void Validate(ref bool value)
        {
            if (!value && !_showWatchingScreen && !_showKeyboard && !_showAway)
            {
                value = true;
            }else if (!value && !_showMouse && !_showKeyboard && !_showAway)
            {
                value = true;
            }
            else if (!value && !_showWatchingScreen && !_showMouse && !_showAway)
            {
                value = true;
            }
            else if (!value && !_showWatchingScreen && !_showKeyboard && !_showMouse)
            {
                value = true;
            }
        }

        public bool ShowMouse
        {
            get => _showMouse;
            set
            {
                Validate(ref value);
                SetProperty(ref _showMouse, value);
            }
        }

        public bool ShowWatchingScreen
        {
            get => _showWatchingScreen;
            set
            {
                Validate(ref value);
                SetProperty(ref _showWatchingScreen, value);
            }
        }

        public bool ShowKeyboard
        {
            get => _showKeyboard;
            set
            {
                Validate(ref value);
                SetProperty(ref _showKeyboard, value);
            }
        }

        public bool ShowAway
        {
            get => _showAway;
            set
            {
                Validate(ref value);
                SetProperty(ref _showAway, value);
            }
        }
    }
}
