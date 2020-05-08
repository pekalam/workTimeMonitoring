using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WMAlghorithm;

namespace MouseKeyboardMonitorServiceTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MouseKeyboardMonitorService _service = new MouseKeyboardMonitorService();

        public MainWindow()
        {
            InitializeComponent();
            _service.MouseMoveAction.ObserveOn(SynchronizationContext.Current).Subscribe((e) => Console.WriteLine($"Mouse action: {e.TotalTimeMs}"));
            _service.KeyboardAction.ObserveOn(SynchronizationContext.Current).Subscribe((e) => Console.WriteLine($"Keyboard action: {e.TotalTimeMs}"));
            _service.Start();
        }
    }
}
