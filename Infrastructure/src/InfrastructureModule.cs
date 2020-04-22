using System;
using System.Diagnostics;
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
using Infrastructure.Messaging;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.src.Repositories;
using Infrastructure.src.Services;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using Serilog;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using Unity;
using WorkTimeAlghorithm;

namespace Infrastructure
{
    public class InfrastructureModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            // System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(SharedFaceRecognitionModel)
            //     .TypeHandle);

            containerProvider.Resolve<IEventAggregator>().GetEvent<InfrastructureModuleLoaded>().Publish(this);
        }

        private object SettingsFactory<T>(IUnityContainer container) where T : new()
        {
            var conf = container.Resolve<IConfigurationService>();
            return conf.Get<T>(nameof(T));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            containerRegistry.RegisterInstance<ILogger>(Log.Logger);

            containerRegistry.RegisterInstance<IMapper>(new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DbTestImageProfile>();
                cfg.AddProfile<DbEventProfile>();
            }).CreateMapper());

            containerRegistry.RegisterInstance(typeof(IConfigurationService),
                new ConfigurationService("settings.json"));
            containerRegistry.GetContainer()
                .RegisterFactory<HeadPositionServiceSettings>(SettingsFactory<HeadPositionServiceSettings>);


            containerRegistry.GetContainer().RegisterType<ICaptureService, CaptureService>();
            containerRegistry.GetContainer().RegisterType<IHeadPositionService, HeadPositionService>();
            containerRegistry.GetContainer().RegisterType<IHcFaceDetection, HcFaceDetection>();
            containerRegistry.GetContainer().RegisterType<IDnFaceRecognition, DnFaceRecognition>();
            containerRegistry.GetContainer().RegisterSingleton<ITestImageRepository, SqliteTestImageRepository>();
            containerRegistry.GetContainer()
                .RegisterSingleton<IMouseKeyboardMonitorService, MouseKeyboardMonitorService>();
            containerRegistry.GetContainer().RegisterType<IWorkTimeUow, WorkTimeUow>();
            containerRegistry.GetContainer().RegisterType<IWorkTimeEsRepository, SqliteWorkTimeEsRepository>();
            containerRegistry.GetContainer()
                .RegisterType<IWorkTimeIdGeneratorService, SqliteWorkTimeIdGeneratorService>();
            containerRegistry.GetContainer().RegisterType<IAuthDataRepository, SqliteAuthDataRepository>();
            containerRegistry.GetContainer().RegisterType<IUserRepository, SqliteUserRepository>();

            containerRegistry.GetContainer().RegisterSingleton<WorkTimeEventService>();


            containerRegistry.GetContainer().RegisterSingleton<IAuthenticationService, AuthenticationService>();


            containerRegistry.GetContainer().RegisterFactory<Notifier>((container, type, arg3) =>
            {
                return new Notifier(cfg =>
                {
                    cfg.PositionProvider = new WindowPositionProvider(
                        parentWindow: Application.Current.MainWindow,
                        corner: Corner.BottomRight,
                        offsetX: 0,
                        offsetY: 0);

                    cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                        notificationLifetime: TimeSpan.FromSeconds(10),
                        maximumNotificationCount: MaximumNotificationCount.FromCount(5));

                    cfg.Dispatcher = Application.Current.Dispatcher;
                });
            });



            GlobalExceptionHandler.Init();
        }


    }

    public static class GlobalExceptionHandler
    {
        internal static void Init()
        {
            Application.Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        }

        public static void Handle(Exception e)
        {
            Log.Logger.Fatal(e, "Unhandled exception");
        }

        public static void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Logger.Fatal(e.Exception, "Unhandled exception");
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