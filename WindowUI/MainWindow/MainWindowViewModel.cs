using System.Windows;
using Prism.Mvvm;
using Prism.Regions;

namespace WindowUI.MainWindow
{
    public class MainWindowViewModel : BindableBase, INavigationAware
    {
        private readonly IMainViewController _controller;
        private Visibility _loadingVisibility;

        public MainWindowViewModel(IMainViewController controller)
        {
            _controller = controller;
        }

        public Visibility LoadingVisibility
        {
            get => _loadingVisibility;
            set => SetProperty(ref _loadingVisibility, value);
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