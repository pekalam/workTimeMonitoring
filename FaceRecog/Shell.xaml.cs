using System;
using CommonServiceLocator;
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
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<AppStartedEvent>().Publish(this);
        }
    }
}