using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AutoMapper;
using CommonServiceLocator;
using Domain.Repositories;
using Domain.Services;
using Domain.User;
using Infrastructure.Db;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.src;
using Infrastructure.src.Repositories;
using Infrastructure.src.Services;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using Serilog;
using Serilog.Events;
using UI.Common;
using UI.Common.Messaging;
using Unity;
using WorkTimeAlghorithm;

namespace Infrastructure
{
    public class InfrastructureModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            ModuleInit.OnInitialized(containerProvider);

            containerProvider.Resolve<IEventAggregator>().GetEvent<InfrastructureModuleLoaded>().Publish(this);
        }

        private object SettingsFactory<T>(IUnityContainer container) where T : class,new()
        {
            var conf = container.Resolve<IConfigurationService>();
            return conf.Get<T>(nameof(T));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            LoggingConfiguration.Configure();
            containerRegistry.RegisterInstance<ILogger>(Log.Logger);

            containerRegistry.RegisterInstance<IMapper>(new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DbTestImageProfile>();
                cfg.AddProfile<DbEventProfile>();
            }).CreateMapper());

            containerRegistry.RegisterInstance(typeof(IConfigurationService),
                new ConfigurationService("settings.json"));


            containerRegistry.GetContainer().RegisterType<IAuthDataRepository, SqliteAuthDataRepository>();
            containerRegistry.GetContainer().RegisterType<IUserRepository, SqliteUserRepository>();
            containerRegistry.GetContainer().RegisterSingleton<IAuthenticationService, AuthenticationService>();
            containerRegistry.GetContainer().RegisterSingleton<ITestImageRepository, SqliteTestImageRepository>();
            containerRegistry.GetContainer().RegisterType<IWorkTimeEsRepository, SqliteWorkTimeEsRepository>();
            containerRegistry.GetContainer().RegisterType<IWorkTimeUow, WorkTimeUow>();
            containerRegistry.GetContainer().RegisterType<IWorkTimeIdGeneratorService, SqliteWorkTimeIdGeneratorService>();


            GlobalExceptionHandler.Init();
            ModuleInit.Init(containerRegistry);
        }


    }

    public static class GlobalExceptionHandler
    {
        internal static void Init()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        }

        public static void Handle(Exception e)
        {
            Log.Logger.Fatal(e, "Unhandled exception");
        }

        public static void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.Logger.Fatal(e.Exception, "Unhandled exception");
        }

        public static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Logger.Fatal(e.ExceptionObject as Exception, "Unhandled exception");
        }
    }
}