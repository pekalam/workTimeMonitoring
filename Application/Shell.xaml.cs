using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CommonServiceLocator;
using Gma.System.MouseKeyHook;
using Infrastructure;
using Infrastructure.Messaging;
using MahApps.Metro.Controls;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
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
            ni.Icon = new System.Drawing.Icon(@"C:\Users\Marek Pękala\Desktop\fu.ico");
            ni.Visible = true;
            ni.DoubleClick +=
                delegate
                {
                    ShowWindow();
                };


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