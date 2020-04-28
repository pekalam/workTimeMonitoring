using System.Windows.Controls;

namespace WindowUI.Statistics
{
    /// <summary>
    /// Interaction logic for StatisticsView
    /// </summary>
    public partial class StatisticsView : UserControl
    {
        public StatisticsView()
        {
            InitializeComponent();
        }

        private void MetroAnimatedSingleRowTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (DataContext as StatisticsViewModel).RaiseTabChanged(tab.SelectedIndex);
        }
    }
}