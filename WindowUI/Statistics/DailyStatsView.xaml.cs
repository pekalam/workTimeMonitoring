using System.Windows.Controls;

namespace WindowUI.Statistics
{
    /// <summary>
    /// Interaction logic for DailyStatsView.xaml
    /// </summary>
    public partial class DailyStatsView : UserControl
    {
        public DailyStatsView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            (DataContext as DailyStatsViewModel).IsDirty = true;
        }
    }
}
