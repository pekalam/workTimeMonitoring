using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Infrastructure;
using Infrastructure.Repositories;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Prism.Mvvm;
using Prism.Regions;
using WindowUI.FaceInitialization;

namespace WindowUI.MainWindow
{
    public enum NavigationItems
    {

    }

    /// <summary>
    /// Interaction logic for MainWindowView
    /// </summary>
    public partial class MainWindowView : UserControl
    {
        public MainWindowView()
        {
            InitializeComponent();
        }

        private void HamburgerMenuControl_HamburgerButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!HamburgerMenuControl.IsPaneOpen)
            {
                HamburgerMenuControl.Width = HamburgerMenuControl.OpenPaneLength;
            }
            else
            {
                HamburgerMenuControl.Width = HamburgerMenuControl.CompactPaneLength;
            }
        }

        private void HamburgerMenuControl_ItemInvoked(object sender, MahApps.Metro.Controls.HamburgerMenuItemInvokedEventArgs args)
        {
            var menuItem = (args.InvokedItem as HamburgerMenuItem);
            Debug.Assert(menuItem != null);
            Debug.Assert(menuItem.Tag != null);
            var navItemVm = (menuItem.Tag as NavigationItemViewModel);
            
            if (navItemVm == null)
            {
                throw new ArgumentException("Null tag value of menu item");
            }


            if (ModuleCommands.Navigate.CanExecute(navItemVm.NavigationItem))
            {
                
            }

        }
    }

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

    public class MainWindowViewModel : BindableBase, INavigationAware
    {
        private IMainViewController _controller;

        public MainWindowViewModel(IMainViewController controller)
        {
            _controller = controller;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _controller.Init(this);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }



    public class NavigationItemViewModel : BindableBase
    {
        public NavigationItems NavigationItem { get; set; }
        public string Label { get; set; }
        public string IconName { get; set; }
    }
}
