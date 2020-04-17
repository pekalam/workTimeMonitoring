using Prism.Mvvm;
using Prism.Regions;

namespace WindowUI.MainWindow
{
    public class MainWindowViewModel : BindableBase, INavigationAware
    {
        private readonly IMainViewController _controller;

        public MainWindowViewModel(IMainViewController controller)
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
}