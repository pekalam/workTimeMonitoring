using Domain.User;
using MahApps.Metro.Controls.Dialogs;
using Prism.Regions;
using System.Windows;
using System.Windows.Threading;
using Prism.Events;
using UI.Common;
using WindowUI.FaceInitialization;
using WMAlghorithm;
using WMAlghorithm.Services;

namespace WindowUI.MainWindow
{
    public interface IMainViewController
    {
        void Init(MainWindowViewModel vm);
    }

    public class MainViewController : IMainViewController
    {
        private MainWindowViewModel? _vm;
        private readonly ITestImageRepository _testImageRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly IRegionManager _regionManager;
        private readonly AlgorithmService _algorithmService;
        private readonly IEventAggregator _ea;


        public MainViewController(ITestImageRepository testImageRepository, IRegionManager regionManager, IAuthenticationService authenticationService,  AlgorithmService algorithmService, IEventAggregator ea)
        {
            _regionManager = regionManager;
            _authenticationService = authenticationService;
            _algorithmService = algorithmService;
            _ea = ea;
            _testImageRepository = testImageRepository;
        }

        public void Init(MainWindowViewModel vm)
        {
            _vm = vm;

            _ea.GetEvent<LoadNotificationsModuleEvent>().Publish();
            //todo
            if (!_algorithmService.TryRestore())
            {
                Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                {
                    if (ShouldStartInitFaceStep())
                    {
                        if (ShowInitFaceStepDialog())
                        {
                            _regionManager.Regions[ShellRegions.MainRegion].RequestNavigate(nameof(FaceInitializationView));
                        }
                        else
                        {
                            Application.Current.Shutdown();
                        }
                    }
                });
            }

        }

        private bool ShouldStartInitFaceStep()
        {
            var user = _authenticationService.User;
            if (_testImageRepository.GetReferenceImages(user).Count >= InitFaceService.MinImages)
            {
                return false;
            }

            return true;
        }

        private bool ShowInitFaceStepDialog()
        {
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Start",
                NegativeButtonText = "Cancel",
            };
            var result = WindowModuleStartupService.ShellWindow.ShowModalMessageExternal("Action required",
                "You must go through profile initialization step.",
                MessageDialogStyle.AffirmativeAndNegative, mySettings);

            return result == MessageDialogResult.Affirmative;
        }
    }
}