using Prism.Mvvm;

namespace WindowUI.MainWindow
{
    public class NavigationItemViewModel : BindableBase
    {
        public NavigationItems NavigationItem { get; set; }
        public string Label { get; set; }
        public string IconName { get; set; }
    }
}