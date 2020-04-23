using System;
using System.Threading.Tasks;
using System.Windows;
using CommonServiceLocator;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using Unity;
using WindowUI.FaceInitialization;
using WindowUI.LoginWindow;
using WindowUI.MainWindow;
using WindowUI.Profile;
using WindowUI.StartWork;
using WindowUI.Statistics;

namespace WindowUI
{
    public class WindowUiModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<LoginView>();
            containerRegistry.RegisterForNavigation<MainWindowView>();
            containerRegistry.RegisterForNavigation<FaceInitializationView>();
            containerRegistry.RegisterForNavigation<StartWorkView>();
            containerRegistry.RegisterForNavigation<StatisticsView>();
            containerRegistry.RegisterForNavigation<ProfileView>();

            containerRegistry.GetContainer()
                .RegisterType<IFaceInitializationController, FaceInitializationController>();

            containerRegistry.GetContainer()
                .RegisterType<IMainViewController, MainViewController>();

            containerRegistry.GetContainer()
                .RegisterType<ILoginViewController, LoginViewController>();

            containerRegistry.GetContainer()
                .RegisterType<IProfileViewController, ProfileViewController>();

            containerRegistry.GetContainer().RegisterSingleton<WorkTimeModuleService>();

            containerRegistry.GetContainer().RegisterInstance(ServiceLocator.Current.GetInstance<WindowModuleStartupService>());
        }
    }
}