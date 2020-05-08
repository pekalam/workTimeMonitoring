using Domain.Services;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using Unity;
using WMAlghorithm;

namespace UI.Common
{
    public class UiCommonModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            UnityBootstraper.Init(containerRegistry.GetContainer());
            containerRegistry.GetContainer().RegisterSingleton<WorkTimeEventService>();
        }
    }
}