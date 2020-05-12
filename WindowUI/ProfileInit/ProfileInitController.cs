using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Domain.User;
using Prism.Commands;
using Prism.Regions;
using UI.Common;
using UI.Common.Extensions;
using WindowUI.MainWindow;
using WMAlghorithm;
using WMAlghorithm.Services;

namespace WindowUI.ProfileInit
{
    public interface IProfileInitController
    {
        Task Init(ProfileInitViewModel vm);
        ICommand StepInfoContinueClick { get; }
        ICommand StepInfoRetryClick { get; }
        ICommand StartFaceInitCommand { get; }
        ICommand BackCommand { get; }
    }
    //todo time init 3s
    public class ProfileInitController : IProfileInitController
    {
        private ICommand _userPanelNavigation;
        private ProfileInitViewModel _vm;
        private readonly ICaptureService _captureService;
        private readonly IHcFaceDetection _faceDetection;
        private readonly ITestImageRepository _testImageRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly IRegionManager _regionManager;
        private readonly ProfileInitService _profileInitService;
        private bool _startStep;
        private bool _stepCompleted;
        private CancellationTokenSource _camCts;

        public ProfileInitController(ICaptureService captureService, IHcFaceDetection faceDetection,
            ProfileInitService profileInitService, ITestImageRepository testImageRepository,
            IAuthenticationService authenticationService, IRegionManager regionManager)
        {
            _captureService = captureService;
            _faceDetection = faceDetection;
            _profileInitService = profileInitService;
            _testImageRepository = testImageRepository;
            _authenticationService = authenticationService;
            _regionManager = regionManager;
            StepInfoContinueClick = new DelegateCommand(BackExecute);
            StepInfoRetryClick = new DelegateCommand(StepInfoRetryExecute);
            StartFaceInitCommand = new DelegateCommand(StartFaceInitExecute);
            BackCommand = new DelegateCommand(BackExecute);
            _profileInitService.InitFaceProgress = new Progress<ProfileInitProgressArgs>(OnInitFaceProgress);
        }

        private void BackExecute()
        {
            _camCts.Cancel();
            WindowUiModuleCommands.NavigateProfile.UnregisterCommand(_userPanelNavigation);
            _regionManager.Regions[ShellRegions.MainRegion].Remove(_regionManager.Regions[ShellRegions.MainRegion].ActiveViews.First());
            _regionManager.Regions[ShellRegions.MainRegion].RequestNavigate(nameof(MainWindowView));
        }

        private void StartFaceInitExecute()
        {
            _startStep = true;
        }

        private void StepInfoRetryExecute()
        {
            _profileInitService.Reset();
            _vm.Progress = 0;
            _vm.HidePhotoPreview();
            _vm.HideStepInfo();
            _vm.StepStarted = false;
            _stepCompleted = false;
        }

        private void LockUserPanelNavigation()
        {
            _userPanelNavigation = new DelegateCommand(() => { }, () => false);
            WindowUiModuleCommands.NavigateProfile.RegisterCommand(_userPanelNavigation);
        }

        public async Task Init(ProfileInitViewModel vm)
        {
            _vm = vm;
            _vm.ShowOverlay("Initializing camera...");

            LockUserPanelNavigation();

            await Task.Run(async () =>
            {
                await StartInitFaceStep();
            });
        }

        public ICommand StepInfoContinueClick { get; }
        public ICommand StepInfoRetryClick { get; }
        public ICommand StartFaceInitCommand { get; }
        public ICommand BackCommand { get; }

        private async Task StartInitFaceStep()
        {
            _camCts = new CancellationTokenSource();
            var camEnumerator = _captureService.CaptureFrames(_camCts.Token).GetAsyncEnumerator(_camCts.Token);
            Task? stepEndTask = null;

            while (await camEnumerator.MoveNextAsync())
            {
                using var frame = camEnumerator.Current;
                Application.Current.Dispatcher.Invoke(() => _vm.CallOnFrameChanged(frame.ToBitmapImage()));

                var rects = _faceDetection.DetectFrontalThenProfileFaces(frame);

                if (_startStep)
                {
                    if (rects.Length == 1)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _vm.HideOverlay();
                            _vm.StepStarted = true;
                        });
                       
                        stepEndTask = await _profileInitService.InitFace(_authenticationService.User, camEnumerator,
                            _camCts.Token);

                        _startStep = false;
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => _vm.ShowOverlay(rects.Length > 1 ? "More than 1 face detected" : "Face not detected"));
                    }
                }


                if (rects.Length == 1)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _vm.HideOverlay();
                        _vm.CallOnFaceDetected(rects.First());
                    });
                      
                }
                else
                {
                    if (!_stepCompleted)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _vm.ShowOverlay(rects.Length > 1 ? "More than 1 face detected" : "Face not detected");
                            _vm.CallOnNoFaceDetected();
                        });
                        
                    }
                }
            }

            if (stepEndTask != null)
            {
                await stepEndTask;
            }
        }

        private void HandleFaceProgress(ProfileInitProgressArgs obj)
        {
            switch (obj.ProgressState)
            {
                case ProfileInitProgress.FaceNotDetected:
                    _vm.ShowErrorStepInfo("Face not detected");
                    break;
                case ProfileInitProgress.FaceNotStraight:
                    _vm.ShowInstructions("Look at the front of cam");
                    break;
                case ProfileInitProgress.FaceNotTurnedLeft:
                    _vm.ShowInstructions("Look in the following direction", rightArrow: true);
                    break;
                case ProfileInitProgress.FaceNotTurnedRight:
                    _vm.ShowInstructions("Look in the following direction", leftArrow: true);
                    break;
                case ProfileInitProgress.FaceRecognitionError:
                    _vm.ShowErrorStepInfo("Invalid photos");
                    break;
                case ProfileInitProgress.PhotosTaken:
                    _vm.HideInstructions();
                    break;
                case ProfileInitProgress.Progress:
                    _vm.HideStepInfo();
                    if (obj.ProgressPercentage == 100)
                    {
                        _vm.ShowSuccessStepInfo("Profile initialized");
                        _vm.ShowPhotoPreview(_testImageRepository.GetAll(_authenticationService.User)
                            .Select(p => p.Img.ToBitmapImage())
                            .ToArray());
                        _stepCompleted = true;
                    }

                    break;
            }
        }

        private void OnInitFaceProgress(ProfileInitProgressArgs obj)
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