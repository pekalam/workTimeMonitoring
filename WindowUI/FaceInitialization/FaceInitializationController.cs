using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Infrastructure;
using Infrastructure.Repositories;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using WorkTimeAlghorithm;

namespace WindowUI.FaceInitialization
{
    public interface IFaceInitializationController
    {
        Task Init(FaceInitializationViewModel vm);
        ICommand StepInfoContinueClick { get; }
        ICommand StepInfoRetryClick { get; }
        ICommand StartFaceInitCommand { get; }
    }

    public class FaceInitializationController : IFaceInitializationController
    {
        private FaceInitializationViewModel _vm;
        private readonly ICaptureService _captureService;
        private readonly IHcFaceDetection _faceDetection;
        private readonly ITestImageRepository _testImageRepository;
        private readonly InitFaceService _initFaceService;
        private bool _startStep;
        private bool _stepCompleted;
        private CancellationTokenSource _camCts;

        public FaceInitializationController(ICaptureService captureService, IHcFaceDetection faceDetection,
            InitFaceService initFaceService, ITestImageRepository testImageRepository)
        {
            _captureService = captureService;
            _faceDetection = faceDetection;
            _initFaceService = initFaceService;
            _testImageRepository = testImageRepository;
            StepInfoContinueClick = new DelegateCommand(OnStepInfoContinueClick);
            StepInfoRetryClick = new DelegateCommand(OnStepInfoRetryClick);
            StartFaceInitCommand = new DelegateCommand(OnStartFaceInit);
            _initFaceService.InitFaceProgress = new Progress<InitFaceProgressArgs>(OnInitFaceProgress);
        }

        private void OnStartFaceInit()
        {
            _startStep = true;
        }

        private void OnStepInfoRetryClick()
        {
            _vm.HidePhotoPreview();
            _vm.StepStarted = false;
            _stepCompleted = false;
        }

        private void OnStepInfoContinueClick()
        {
            _camCts.Cancel();
        }

        public async Task Init(FaceInitializationViewModel vm)
        {
            _vm = vm;
            _vm.ShowOverlay("Initializing camera...");




            await StartInitFaceStep();
        }

        public ICommand StepInfoContinueClick { get; }
        public ICommand StepInfoRetryClick { get; }
        public ICommand StartFaceInitCommand { get; }

        private async Task StartInitFaceStep()
        {
            _camCts = new CancellationTokenSource();
            var stepCts = new CancellationTokenSource();
            var camEnumerator = _captureService.CaptureFrames(_camCts.Token).GetAsyncEnumerator(_camCts.Token);
            Task stepEndTask = null;

            while (await camEnumerator.MoveNextAsync().ConfigureAwait(true))
            {
                using var frame = camEnumerator.Current;
                _vm.CallOnFrameChanged(frame.ToBitmapImage());

                var rects = _faceDetection.DetectFrontalThenProfileFaces(frame);

                if (_startStep)
                {
                    if (rects.Length == 1)
                    {
                        _vm.HideOverlay();
                        _vm.StepStarted = true;
                        stepEndTask = await _initFaceService.InitFace(camEnumerator, stepCts.Token)
                            .ConfigureAwait(true);

                        _startStep = false;
                    }
                    else
                    {
                        _vm.ShowOverlay("Waiting for face...");
                    }
                }


                if (rects.Length == 1)
                {
                    _vm.HideOverlay();
                    _vm.CallOnFaceDetected(rects.First());
                }
                else
                {
                    if (!_stepCompleted)
                    {
                        _vm.ShowOverlay("Waiting for face...");
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
                    _vm.ShowErrorStepInfo("Face not detected");
                    break;
                case InitFaceProgress.FaceNotStraight:
                    _vm.ShowInformationStepInfo("Invalid face position");
                    break;
                case InitFaceProgress.FaceRecognitionError:
                    _vm.ShowErrorStepInfo("Invalid face");
                    break;
                case InitFaceProgress.Progress:
                    _vm.HideStepInfo();
                    if (obj.ProgressPercentage == 100)
                    {
                        _vm.ShowSuccessStepInfo("Profile initialized");
                        _vm.ShowPhotoPreview(_testImageRepository.GetAll().Select(p => p.Img.ToBitmapImage())
                            .ToArray());
                        _stepCompleted = true;
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
                _vm.CallOnFrameChanged(obj.Frame.ToBitmapImage());
                if (obj.FaceRect.HasValue)
                {
                    _vm.CallOnFaceDetected(obj.FaceRect.Value);
                }
                else
                {
                    _vm.CallOnNoFaceDetected();
                }
            }
        }
    }
}