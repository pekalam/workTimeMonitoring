using System;
using System.ComponentModel;
using System.Windows;
using CommonServiceLocator;
using Gma.System.MouseKeyHook;
using Infrastructure;
using Infrastructure.Messaging;
using MahApps.Metro.Controls;
using Prism.Events;

namespace Application
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : AcrylicWindow
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
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };


            System.Windows.Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
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