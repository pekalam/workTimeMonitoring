using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Infrastructure;
using NotifyBarUI;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using Serilog;
using WindowUI;

namespace FaceRecog
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
            moduleCatalog.AddModule<WindowUIModule>();
            moduleCatalog.AddModule<NotifyBarUIModule>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<Shell>();
        }
    }
}
