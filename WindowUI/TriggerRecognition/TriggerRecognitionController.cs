using Domain.User;
using MahApps.Metro.Controls.Dialogs;
using OpenCvSharp;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Prism.Events;
using Serilog;
using UI.Common;
using UI.Common.Extensions;
using WindowUI.MainWindow;
using WMAlghorithm;
using WMAlghorithm.Services;
using WMAlghorithm.StateMachine;

namespace WindowUI.TriggerRecognition
{
    public class NullableToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rev = false;
            if (parameter != null)
            {
                rev = System.Convert.ToBoolean(parameter);
            }

            if (!(value is bool b))
            {
                return Visibility.Hidden;
            }

            return rev ? !b == true ? Visibility.Visible : Visibility.Hidden : b == true ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TriggerRecognitionController : ITriggerRecognitionController
    {
        private TriggerRecognitionViewModel _vm;
        private readonly IRegionManager _rm;
        private readonly ManualRecogTriggerService _recogTriggerService;
        private readonly Progress<ManualRecogProgress> _progress;
        private readonly IEventAggregator _ea;

        public TriggerRecognitionController(IRegionManager rm, ManualRecogTriggerService recogTriggerService, IEventAggregator ea)
        {
            _rm = rm;
            _recogTriggerService = recogTriggerService;
            _ea = ea;
            _progress = new Progress<ManualRecogProgress>(RecogProgressHandler);
        }

        private MessageDialogResult ShowRetryDialog()
        {
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Yes",
                NegativeButtonText = "No",
            };
            return WindowModuleStartupService.ShellWindow.ShowModalMessageExternal("Recognition failed",
                "Try again?",
                MessageDialogStyle.AffirmativeAndNegative, mySettings);
        }


        private void RecogProgressHandler(ManualRecogProgress arg)
        {
            switch (arg.State)
            {
                case ManualRecogState.FrameCap:
                    _vm.CallOnFrameChanged(arg.Frame);
                    _vm.HideLoading();
                    break;
                case ManualRecogState.FaceDetected:
                    _vm.CallOnFaceDetected(arg.Face.Value);
                    break;
                case ManualRecogState.NoFaceDetected:
                    _vm.CallOnNoFaceDetected();
                    break;
                case ManualRecogState.RecogStarted:
                    _vm.ShowLoading();
                    break;
                case ManualRecogState.RecogFinished:
                    _vm.Loading = false;
                    break;
            }
        }

        public async Task Init(TriggerRecognitionViewModel vm, bool windowOpened, object previousView)
        {
            void RestoreWindowState()
            {
                _rm.Regions[ShellRegions.MainRegion].RemoveActiveView();
                if (previousView != null && _rm.Regions[ShellRegions.MainRegion].Views.Contains(previousView))
                {
                    _rm.Regions[ShellRegions.MainRegion].Activate(previousView);
                }
                else
                {
                    _rm.Regions[ShellRegions.MainRegion].RequestNavigate(nameof(MainWindowView));
                }

                if (!windowOpened)
                {
                    Application.Current.Dispatcher.InvokeAsync(() => WindowModuleStartupService.ShellWindow.Hide());
                }
            }

            _ea.GetEvent<HideNotificationsEvent>().Publish();
            _vm = vm;
            _vm.ShowLoading();


            await Task.Run(async () =>
            {
                var vmDispatch = new TriggerRecognitionVmDispatcherDecorator(_vm);

                bool recognized = false;
                do
                {
                    try
                    {
                        recognized = await _recogTriggerService.StartRecognition(_progress);
                    }
                    catch (CamLockedException e)
                    {
                        Log.Logger.Debug(e, "TriggerRecog exception");
                        RestoreWindowState();
                        return;
                    }
                    if (!recognized)
                    {
                        vmDispatch.ShowRecognitionFailure();
                        await Task.Delay(1500);
                        vmDispatch.ResetRecognition();
                    }

                } while (!recognized && ShowRetryDialog() == MessageDialogResult.Affirmative);

                if (recognized)
                {
                    vmDispatch.ShowRecognitionSuccess();
                }
            });

            await Task.Delay(1000).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RestoreWindowState();
                });

            });

        }
    }
}