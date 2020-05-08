using Prism.Commands;

namespace Application
{
    public static class AppCommands
    {
        public static DelegateCommand ShutdownCommand { get; set; } = new DelegateCommand(() => System.Windows.Application.Current.Shutdown());
    }
}
