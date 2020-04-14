using System;
using System.Threading.Tasks;
using AutoMapper;
using Infrastructure.Db;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.WorkTime;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using Serilog;
using Unity;

namespace Infrastructure
{
    public class InfrastructureModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        private object SettingsFactory<T>(IUnityContainer container) where T : new()
        {
            var conf = container.Resolve<ConfigurationService>();
            return conf.Get<T>(nameof(T));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            containerRegistry.RegisterInstance<ILogger>(Log.Logger);

            containerRegistry.RegisterInstance<IMapper>(new MapperConfiguration(cfg => cfg.AddProfile<DbTestImageProfile>()).CreateMapper());

            containerRegistry.RegisterInstance(typeof(ConfigurationService), new ConfigurationService("settings.json"));
            containerRegistry.GetContainer()
                .RegisterFactory<HeadPositionServiceSettings>(SettingsFactory<HeadPositionServiceSettings>);

            containerRegistry.GetContainer().RegisterType<ICaptureService, CaptureService>();
            containerRegistry.GetContainer().RegisterType<IHeadPositionService, HeadPositionService>();
            containerRegistry.GetContainer().RegisterType<IHcFaceDetection, HcFaceDetection>();
            containerRegistry.GetContainer().RegisterType<IDnFaceRecognition, DnFaceRecognition>();
            containerRegistry.GetContainer().RegisterSingleton<ITestImageRepository, DefaultTestImageRepository>();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        }

        private void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.Logger.Fatal(e.Exception, "Unhandled exception");
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Logger.Fatal(e.ExceptionObject as Exception, "Unhandled exception");
        }
    }
}