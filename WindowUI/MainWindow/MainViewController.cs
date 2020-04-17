using System.Windows;
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
        private readonly IRegionManager _regionManager;

        public MainViewController(ITestImageRepository testImageRepository, IRegionManager regionManager)
        {
            _regionManager = regionManager;
            _testImageRepository = testImageRepository;
        }

        public void Init(MainWindowViewModel vm)
        {
            _vm = vm;

            if (_testImageRepository.Count != 3)
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

            return true;
        }
    }
}