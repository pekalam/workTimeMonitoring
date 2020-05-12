using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.User;
using OpenCvSharp;
using WMAlghorithm.StateMachine;

namespace WMAlghorithm.Services
{
    public enum ManualRecogState
    {
        RecogStarted, RecogFinished, FaceDetected , NoFaceDetected, FrameCap
    }

    public class ManualRecogProgress
    {
        public ManualRecogState State { get; set; }
        public Rect? Face { get; set; }
        public Mat? Frame { get; set; }
    }

    public class CamLockedException : Exception
    {
    }

    public class ManualRecogTriggerService
    {
        private const double SecWait = 3.0; 

        private readonly ICaptureService _captureService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IHcFaceDetection _faceDetection;
        private readonly AlghorithmFaceRecognition _faceRecognition;
        private readonly AlgorithmService _algService;

        private CancellationTokenSource _camCts;
        private IAsyncEnumerator<Mat> _camEnumerator;
        private IProgress<ManualRecogProgress> _progress;

        public ManualRecogTriggerService(AlgorithmService algService, AlghorithmFaceRecognition faceRecognition, IHcFaceDetection faceDetection, IAuthenticationService authenticationService, ICaptureService captureService)
        {
            _algService = algService;
            _faceRecognition = faceRecognition;
            _faceDetection = faceDetection;
            _authenticationService = authenticationService;
            _captureService = captureService;
        }


        private async Task<bool> Init(IProgress<ManualRecogProgress> progress)
        {
            var initialized = true;
            _progress = progress;
            if (_camCts == null)
            {
                _camCts = new CancellationTokenSource();
            }

            if (_camEnumerator == null)
            {
                _camEnumerator = _captureService.CaptureFrames(_camCts.Token).GetAsyncEnumerator(_camCts.Token);
            }

            if (!_algService.Alghorithm.ManualRecog)
            {
                initialized = await _algService.Alghorithm.StartManualFaceRecog();
            }

            return initialized;
        }

        private async Task<bool> Recognize()
        {
            DateTime? start = null;
            TimeSpan timeElapsed;

            while (await _camEnumerator.MoveNextAsync())
            {
                using var frame = _camEnumerator.Current;
                _progress.Report(new ManualRecogProgress()
                {
                    State = ManualRecogState.FrameCap, Frame = frame
                });

                var faces = _faceDetection.DetectFrontalThenProfileFaces(frame);

                if (faces.Length > 0)
                {
                    _progress.Report(new ManualRecogProgress()
                    {
                        Face = faces[0], State = ManualRecogState.FaceDetected
                    });

                    if (start == null)
                    {
                        start = DateTime.Now;
                        continue;
                    }

                    timeElapsed = DateTime.Now - start.Value;


                    if (timeElapsed.TotalSeconds >= SecWait)
                    {
                        var recogTask = _faceRecognition.RecognizeFace(_authenticationService.User, frame);
                        _progress.Report(new ManualRecogProgress(){State = ManualRecogState.RecogStarted});
                        var (detected, recognized) = await recogTask;
                        _progress.Report(new ManualRecogProgress() { State = ManualRecogState.RecogFinished });
                        if (detected && recognized)
                        {
                            _algService.Alghorithm.SetManualRecogSuccess();
                            _camCts.Cancel();
                        }
                        else if (!recognized)
                        {
                            return false;
                        }

                        start = null;
                    }
                }
                else
                {
                    _progress.Report(new ManualRecogProgress() { State = ManualRecogState.NoFaceDetected });
                    start = null;
                }
            }

            return true;
        }

        public async Task<bool> StartRecognition(IProgress<ManualRecogProgress> progress)
        {
            var initialized = await Init(progress);
            if (!initialized)
            {
                throw new CamLockedException();
            }


            return await Recognize();
        }
    }
}
