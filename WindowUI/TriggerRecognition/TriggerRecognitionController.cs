using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Domain.User;
using Infrastructure;
using Prism.Commands;
using WorkTimeAlghorithm;
using WorkTimeAlghorithm.StateMachine;

namespace WindowUI.TriggerRecognition
{
    public class TriggerRecognitionController : ITriggerRecognitionController
    {
        private const int MaxRetries = 3;

        private readonly ICaptureService _captureService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IHcFaceDetection _faceDetection;
        private readonly AlghorithmFaceRecognition _faceRecognition;
        private TriggerRecognitionViewModel? _vm;
        private CancellationTokenSource? _camCts;
        private readonly WorkTimeModuleService _moduleService;

        public TriggerRecognitionController(ICaptureService captureService, AlghorithmFaceRecognition faceRecognition, IAuthenticationService authenticationService, WorkTimeModuleService moduleService, IHcFaceDetection faceDetection)
        {
            _captureService = captureService;
            _faceRecognition = faceRecognition;
            _authenticationService = authenticationService;
            _moduleService = moduleService;
            _faceDetection = faceDetection;
            Retry = new DelegateCommand(() => { });
        }


        public ICommand Retry { get; }

        public async Task Init(TriggerRecognitionViewModel vm)
        {
            _vm = vm;
            
            _camCts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            var camEnumerator = _captureService.CaptureFrames(_camCts.Token).GetAsyncEnumerator(_camCts.Token);

            var failed = 0;
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
                        var (detected, recognized) = await _faceRecognition.RecognizeFace(_authenticationService.User, frame);

                        if (detected && recognized)
                        {
                            _moduleService.Alghorithm.SetFaceRecog();
                            _camCts.Cancel();
                        }
                        else if (!recognized)
                        {
                            failed++;
                            if (failed == MaxRetries)
                            {
                                _moduleService.Alghorithm.CancelManualFaceRecog();
                                _camCts.Cancel();
                            }
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
        }
    }
}