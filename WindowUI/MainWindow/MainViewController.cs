using System;
using System.Windows;
using System.Windows.Threading;
using Domain.User;
using Infrastructure;
using MahApps.Metro.Controls.Dialogs;
using Prism.Regions;
using WindowUI.FaceInitialization;
using WorkTimeAlghorithm;

namespace WindowUI.MainWindow
{
    public interface IMainViewController
    {
        void Init(MainWindowViewModel vm);
    }

    public class MainViewController : IMainViewController
    {
        private MainWindowViewModel _vm;
        private readonly ITestImageRepository _testImageRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly IRegionManager _regionManager;

        public MainViewController(ITestImageRepository testImageRepository, IRegionManager regionManager, IAuthenticationService authenticationService)
        {
            _regionManager = regionManager;
            _authenticationService = authenticationService;
            _testImageRepository = testImageRepository;
        }

        public void Init(MainWindowViewModel vm)
        {
            _vm = vm;


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

        private bool ShouldStartInitFaceStep()
        {
            var user = _authenticationService.User;
            if (_testImageRepository.GetReferenceImages(user).Count >= 3)
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