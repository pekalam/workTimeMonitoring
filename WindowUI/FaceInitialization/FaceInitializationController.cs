using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Infrastructure;
using Infrastructure.WorkTime;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;

namespace WindowUI.FaceInitialization
{
    public class FaceInitializationController : IFaceInitializationController
    {
        private FaceInitializationViewModel _vm;
        private readonly ICaptureService _captureService;
        private readonly IHcFaceDetection _faceDetection;
        private readonly ITestImageRepository _testImageRepository;
        private readonly InitFaceService _initFaceService;
        private bool _stepError = false;
        private bool _stepCompleted = false;
        private bool _firstRun = true;

        public FaceInitializationController(ICaptureService captureService, IHcFaceDetection faceDetection, InitFaceService initFaceService, ITestImageRepository testImageRepository)
        {
            _captureService = captureService;
            _faceDetection = faceDetection;
            _initFaceService = initFaceService;
            _testImageRepository = testImageRepository;
            StepInfoContinueClick = new DelegateCommand(OnStepInfoContinueClick);
            StepInfoRetryClick = new DelegateCommand(OnStepInfoRetryClick);
        }

        private void OnStepInfoRetryClick()
        {
            _firstRun = true;
        }

        private void OnStepInfoContinueClick()
        {

        }

        public async void Init(FaceInitializationViewModel vm)
        {
            _vm = vm;

            _vm.LoadingOverlayText = "Initializing camera...";
            _vm.LoadingOverlayVisible = true;

            await StartInitFaceStep();
        }

        public ICommand StepInfoContinueClick { get; }
        public ICommand StepInfoRetryClick { get; }

        private async Task StartInitFaceStep()
        {
            var cts = new CancellationTokenSource();
            var stepCts = new CancellationTokenSource();
            var camEnumerator = _captureService.CaptureFrames(cts.Token).GetAsyncEnumerator(cts.Token);
            Task stepEndTask = null;

            while(await camEnumerator.MoveNextAsync().ConfigureAwait(true))
            {
                var frame = camEnumerator.Current;
                _vm.OnFrameChanged?.Invoke(frame.ToBitmapImage());

                var (rects, faces) = _faceDetection.DetectFrontalThenProfileFaces(frame);

                foreach (var rect in rects)
                {
                    _vm.OnFaceDetected?.Invoke(rect);
                }

                if (_firstRun)
                {
                    if (rects.Length == 1)
                    {
                        HideOverlay();
                        if (ShowInitFaceStepDialog())
                        {
                            _initFaceService.InitFaceProgress = new Progress<InitFaceProgressArgs>(OnInitFaceProgress);
                            stepEndTask = await _initFaceService.InitFace(camEnumerator, stepCts.Token).ConfigureAwait(true);
                        }
                        _firstRun = false;
                    }
                    else
                    {
                        ShowOverlay("Waiting for face...");
                    }
                }


                
            }

            if (stepEndTask != null)
            {
                await stepEndTask;
            }
        }

        private void HandleFaceProgress(InitFaceProgressArgs obj)
        {
            switch (obj.ProgressState)
            {
                case InitFaceProgress.FaceNotDetected:
                    ShowErrorStepInfo("Face not detected");
                    break;
                case InitFaceProgress.FaceNotStraight:
                    ShowInformationStepInfo("Invalid face position");
                    break;
                case InitFaceProgress.FaceRecognitionError:
                    ShowErrorStepInfo("Invalid face");
                    _vm.StepInfoContinueVisible = false;
                    _vm.StepInfoRetryVisible = true;
                    _stepError = true;
                    break;
                case InitFaceProgress.Progress:
                    HideStepInfo();
                    if (obj.ProgressPercentage == 100)
                    {
                        ShowSuccessStepInfo("Profile initialized");
                        _stepCompleted = true;
                        _vm.StepInfoContinueVisible = true;
                        _vm.StepInfoRetryVisible = true;
                        _vm.PhotoPreviewVisible = true;
                        var photos = _testImageRepository.GetAll();
                        _vm.Photo1 = photos[0].FaceColor.Img.ToBitmapImage();
                        _vm.Photo2 = photos[1].FaceColor.Img.ToBitmapImage();
                        _vm.Photo3 = photos[2].FaceColor.Img.ToBitmapImage();
                    }
                    break;
            }
        }

        private void OnInitFaceProgress(InitFaceProgressArgs obj)
        {
            Debug.WriteLine(obj.ProgressState);
            Debug.WriteLine(obj.ProgressPercentage);
            _vm.Progress = obj.ProgressPercentage;

            HandleFaceProgress(obj);

            if (obj.Frame != null)
            {
                _vm.OnFrameChanged.Invoke(obj.Frame.ToBitmapImage());
            }

            if (obj.FaceRect.HasValue)
            {
                _vm.OnFaceDetected.Invoke(obj.FaceRect.Value);
            }
        }

        private void ShowOverlay(string text)
        {
            _vm.LoadingOverlayVisible = true;
            _vm.LoadingOverlayText = text;
        }

        private void HideOverlay() => _vm.LoadingOverlayVisible = false;

        private void HideStepInfo() => _vm.StepInfoPanelVisible = false;

        private void ShowErrorStepInfo(string msg)
        {
            _vm.StepInfoPanelBrush = Brushes.Red;
            _vm.StepInfoPanelText = msg;
            _vm.StepInfoPanelVisible = true;
        }

        private void ShowInformationStepInfo(string msg)
        {
            _vm.StepInfoPanelBrush = Brushes.Gray;
            _vm.StepInfoPanelText = msg;
            _vm.StepInfoPanelVisible = true;
        }

        private void ShowSuccessStepInfo(string msg)
        {
            _vm.StepInfoPanelBrush = Brushes.Green;
            _vm.StepInfoPanelText = msg;
            _vm.StepInfoPanelVisible = true;
        }

        private bool ShowInitFaceStepDialog()
        {
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Start",
                NegativeButtonText = "Cancel",
            };
            var result = WindowModuleStartupService.ShellWindow.ShowModalMessageExternal("Action required", "You must go through profile initialization step.",
                MessageDialogStyle.AffirmativeAndNegative, mySettings);

            return true;
        }
    }
}