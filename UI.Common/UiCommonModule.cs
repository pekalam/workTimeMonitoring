using Domain.Services;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using Unity;

namespace UI.Common
{
    public class UiCommonModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            WorkTimeAlghorithm.UnityBootstraper.Init(containerRegistry.GetContainer());
            containerRegistry.GetContainer().RegisterSingleton<WorkTimeEventService>();
        }
    }
}