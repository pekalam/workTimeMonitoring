using System;
using System.Windows;
using System.Windows.Threading;
using Domain.User;
using Infrastructure;
using MahApps.Metro.Controls.Dialogs;
using Prism.Regions;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;
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
        private Notifier _notifier;
        private readonly WorkTimeModuleService _workTimeModuleService;


        public MainViewController(ITestImageRepository testImageRepository, IRegionManager regionManager, IAuthenticationService authenticationService, Notifier notifier, WorkTimeModuleService workTimeModuleService)
        {
            _regionManager = regionManager;
            _authenticationService = authenticationService;
            _notifier = notifier;
            _workTimeModuleService = workTimeModuleService;
            _testImageRepository = testImageRepository;
        }

        public void Init(MainWindowViewModel vm)
        {
            _vm = vm;

            if (_workTimeModuleService.TryRestore())
            {
                _notifier.ShowInformation("Continuing stoppped monitoring");
            }
            else
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