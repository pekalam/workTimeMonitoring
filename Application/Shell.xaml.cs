using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using CommonServiceLocator;
using Gma.System.MouseKeyHook;
using Infrastructure;
using MahApps.Metro.Controls;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using UI.Common;
using UI.Common.Messaging;
using WindowUI.MainWindow;
using WindowUI.Profile;

namespace Application
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : MetroWindow
    {
        public Shell()
        {
            InitializeComponent();

            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "Application." + "wtmico.ico"))
            {
                ni.Icon = new Icon(stream);
            }
            ni.Visible = true;
            ni.Click +=
                delegate
                {
                    ShowWindow();
                };
            ni.Text = "WTM";


            System.Windows.Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;


            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<ShowWindowEvent>()
                .Subscribe(() =>
                {
                    ShowWindow();
                },true);
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }
        
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }



        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<AppStartedEvent>().Publish(this);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }


    }
}