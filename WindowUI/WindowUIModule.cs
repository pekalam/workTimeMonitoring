﻿using CommonServiceLocator;
using Infrastructure;
using Infrastructure.Messaging;
using MahApps.Metro.Controls;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using Unity;
using WindowUI.FaceInitialization;

namespace WindowUI
{
    internal class WindowModuleStartupService
    {
        private readonly IEventAggregator _ea;
        private readonly IRegionManager _regionManager;

        public WindowModuleStartupService(IEventAggregator ea, IRegionManager regionManager)
        {
            _ea = ea;
            _regionManager = regionManager;
            _ea.GetEvent<AppStartedEvent>().Subscribe(OnAppStarted);
        }

        public static MetroWindow ShellWindow { get; private set; }

        private void OnAppStarted(MetroWindow shellWindow)
        {
            ShellWindow = shellWindow;
            _regionManager.Regions[ShellRegions.MainRegion].RequestNavigate(nameof(FaceInitializationView));
        }
    }

    public class WindowUIModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<FaceInitializationView>();
            containerRegistry.GetContainer()
                .RegisterType<IFaceInitializationController, FaceInitializationController>();
            containerRegistry.GetContainer().RegisterInstance(ServiceLocator.Current.GetInstance<WindowModuleStartupService>());
        }
    }
}