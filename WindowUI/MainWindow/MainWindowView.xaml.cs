using CommonServiceLocator;
using MahApps.Metro.Controls;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using WindowUI.Profile;
using WindowUI.Settings;
using WindowUI.StartWork;
using WindowUI.Statistics;

namespace WindowUI.MainWindow
{
    /// <summary>
    /// Interaction logic for MainWindowView
    /// </summary>
    public partial class MainWindowView : UserControl
    {
        private Dictionary<NavigationItems, object> _navCache = new Dictionary<NavigationItems, object>();

        public MainWindowView()
        {
            InitializeComponent();

            WindowUiModuleCommands.NavigateProfile.RegisterCommand(new DelegateCommand(() =>
            {
                HamburgerMenuControl.SelectedOptionsIndex = 0;
            }));
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

        private void SaveView(NavigationItems navItem, IRegionManager rm)
        {
            if (rm.Regions[MainWindowRegions.MainContentRegion].ActiveViews.Any())
            {
                _navCache[navItem] = rm.Regions[MainWindowRegions.MainContentRegion].ActiveViews.First();
            }
        }

        private bool TryRestore(NavigationItems navItem, IRegionManager rm)
        {
            if (_navCache.ContainsKey(navItem))
            {
                rm.Regions[MainWindowRegions.MainContentRegion].Activate(_navCache[navItem]);
                return true;
            }

            return false;
        }

        private void HamburgerMenuControl_ItemInvoked(object sender,
            MahApps.Metro.Controls.HamburgerMenuItemInvokedEventArgs args)
        {
            var menuItem = (args.InvokedItem as HamburgerMenuItem);
            Debug.Assert(menuItem != null);
            Debug.Assert(menuItem.Tag != null);
            var navItemVm = (menuItem.Tag as NavigationItemViewModel);

            if (navItemVm == null)
            {
                throw new ArgumentException("Null tag value of menu item");
            }

            var regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
            switch (navItemVm.NavigationItem)
            {
                case NavigationItems.StartMonitor:
                    if (!TryRestore(NavigationItems.StartMonitor, regionManager))
                    {
                        regionManager.Regions[MainWindowRegions.MainContentRegion]
                            .RequestNavigate(nameof(StartWorkView));
                        SaveView(NavigationItems.StartMonitor, regionManager);
                    }
                    break;
                case NavigationItems.Statistics:
                    if (!TryRestore(NavigationItems.Statistics, regionManager))
                    {
                        regionManager.Regions[MainWindowRegions.MainContentRegion]
                            .RequestNavigate(nameof(StatisticsView));
                        SaveView(NavigationItems.Statistics, regionManager);
                    }
                    break;
                case NavigationItems.Profile:
                    if (!TryRestore(NavigationItems.Profile, regionManager))
                    {
                        regionManager.Regions[MainWindowRegions.MainContentRegion].RequestNavigate(nameof(ProfileView));
                        SaveView(NavigationItems.Profile, regionManager);
                    }
                    break;
                case NavigationItems.Settings:
                    if (!TryRestore(NavigationItems.Settings, regionManager))
                    {
                        regionManager.Regions[MainWindowRegions.MainContentRegion]
                            .RequestNavigate(nameof(SettingsView));
                        SaveView(NavigationItems.Settings, regionManager);
                    }
                    break;
            }
        }
    }
}