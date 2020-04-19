using System.Windows.Controls;
using Prism.Mvvm;
using Prism.Regions;

namespace WindowUI.StartWork
{
    /// <summary>
    /// Interaction logic for StartWorkView
    /// </summary>
    public partial class StartWorkView : UserControl
    {
        public StartWorkView()
        {
            InitializeComponent();
        }
    }

    public class StartWorkViewModel : BindableBase, INavigationAware
    {
        private StartWorkViewController _controller;

        public StartWorkViewModel(StartWorkViewController controller)
        {
            _controller = controller;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _controller.Init(this);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
    }

    public class StartWorkViewController
    {
        private StartWorkViewModel _vm;

        public void Init(StartWorkViewModel vm)
        {
            _vm = vm;
        }
    }
}
