using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Domain.User;
using Infrastructure;
using MahApps.Metro.Controls.Dialogs;
using OpenCvSharp;
using Prism.Commands;
using Prism.Regions;
using WindowUI.MainWindow;
using WorkTimeAlghorithm;
using WorkTimeAlghorithm.StateMachine;

namespace WindowUI.TriggerRecognition
{
    public class TriggerRecognitionController : ITriggerRecognitionController
    {
        private readonly ICaptureService _captureService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IHcFaceDetection _faceDetection;
        private readonly AlghorithmFaceRecognition _faceRecognition;
        private TriggerRecognitionViewModel? _vm;
        private CancellationTokenSource? _camCts;
        private readonly WorkTimeModuleService _moduleService;
        private readonly IRegionManager _rm;

        public TriggerRecognitionController(ICaptureService captureService, AlghorithmFaceRecognition faceRecognition,
            IAuthenticationService authenticationService, WorkTimeModuleService moduleService,
            IHcFaceDetection faceDetection, IRegionManager rm)
        {
            _captureService = captureService;
            _faceRecognition = faceRecognition;
            _authenticationService = authenticationService;
            _moduleService = moduleService;
            _faceDetection = faceDetection;
            _rm = rm;
            Retry = new DelegateCommand(() => { });
        }


        public ICommand Retry { get; }

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

        private async Task<bool> BeginFaceRecognition(IAsyncEnumerator<Mat> camEnumerator)
        {
            DateTime? start = null;
            TimeSpan timeElapsed;

            while (await camEnumerator.MoveNextAsync().ConfigureAwait(true))
            {
                using var frame = camEnumerator.Current;
                _vm.CallOnFrameChanged(frame.ToBitmapImage());


                var faces = _faceDetection.DetectFrontalThenProfileFaces(frame);

                if (faces.Length > 0)
                {
                    _vm.CallOnFaceDetected(faces[0]);

                    if (start == null)
                    {
                        start = DateTime.Now;
                        continue;
                    }

                    timeElapsed = DateTime.Now - start.Value;


                    if (timeElapsed.TotalSeconds >= 3.0)
                    {
                        var (detected, recognized) =
                            await _faceRecognition.RecognizeFace(_authenticationService.User, frame);
                        if (detected && recognized)
                        {
                            _vm.ShowRecognitionSuccess();
                            _moduleService.Alghorithm.SetFaceRecog();
                            _camCts.Cancel();
                        }
                        else if (!recognized)
                        {
                            _vm.ShowRecognitionFailure();
                            await Task.Delay(1500);
                            _vm.ResetRecognition();
                            return false;
                        }

                        start = null;
                    }
                }
                else
                {
                    _vm.CallOnNoFaceDetected();
                    start = null;
                }
            }

            return true;
        }

        public async Task Init(TriggerRecognitionViewModel vm, bool windowOpened)
        {
            _vm = vm;

            _camCts = new CancellationTokenSource();
            var camEnumerator = _captureService.CaptureFrames(_camCts.Token).GetAsyncEnumerator(_camCts.Token);

            bool recognized;
            do
            {
                _vm.ResetRecognition();
                recognized = await BeginFaceRecognition(camEnumerator);

            } while (!recognized && ShowRetryDialog() == MessageDialogResult.Affirmative);

            if (recognized)
            {
                _vm.ShowRecognitionSuccess();
                _moduleService.Alghorithm.CancelManualFaceRecog();
                _camCts.Cancel();
            }

            await Task.Delay(1000);

            if (!windowOpened)
            {
                WindowModuleStartupService.ShellWindow.Hide();
            }
            else
            {
                //todo restore
            }
        }
    }
}