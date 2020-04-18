using System;
using System.Windows;
using Infrastructure;
using NotifyBarUI;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using WindowUI;

namespace Application
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);
            moduleCatalog.AddModule<InfrastructureModule>();
            moduleCatalog.AddModule<WindowUiModule>();
            moduleCatalog.AddModule<NotifyBarUIModule>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        private void PrismApplication_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine("asd");
        }
    }
}
