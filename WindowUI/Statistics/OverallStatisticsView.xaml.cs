using System.Windows.Controls;

namespace WindowUI.Statistics
{
    /// <summary>
    /// Interaction logic for OverallStatisticsView.xaml
    /// </summary>
    public partial class OverallStatisticsView : UserControl
    {
        public OverallStatisticsView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            (DataContext as OverallStatsViewModel).IsDirty = true;
        }
    }
}
