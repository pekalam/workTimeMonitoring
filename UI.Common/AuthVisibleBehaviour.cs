using System;
using System.Windows;
using CommonServiceLocator;
using Domain.User;
using Microsoft.Xaml.Behaviors;
using Prism.Events;
using UI.Common.Messaging;

namespace UI.Common
{
    public class AuthVisibleBehaviour : Behavior<UIElement>
    {
        private IDisposable? _subscription;

        protected override void OnAttached()
        {
            var ea = ServiceLocator.Current.GetInstance<IEventAggregator>();

            ea.GetEvent<InfrastructureModuleLoaded>().Subscribe(_ =>
            {
                var authService = ServiceLocator.Current.GetInstance<IAuthenticationService>();
                _subscription = authService.LoggedInUser.Subscribe(user =>
                {
                    if (user == null)
                    {
                        AssociatedObject.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        AssociatedObject.Visibility = Visibility.Visible;
                    }
                });
            });

        }

        protected override void OnDetaching()
        {
            _subscription?.Dispose();
        }
    }
}
