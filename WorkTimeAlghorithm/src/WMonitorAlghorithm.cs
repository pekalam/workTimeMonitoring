using System.Diagnostics;
using System.Threading.Tasks;
using Serilog;

namespace WorkTimeAlghorithm
{
    public partial class WMonitorAlghorithm
    {
        private IHcFaceDetection _faceDetection;
        private IDnFaceRecognition _faceRecognition;
        private ICaptureService _captureService;
        private IMouseKeyboardMonitorService _mouseKeyboardMonitorService;


        private bool _canCapureMouseKeyboard;

        public WMonitorAlghorithm(IHcFaceDetection faceDetection, IDnFaceRecognition faceRecognition,
            ICaptureService captureService, IMouseKeyboardMonitorService mouseKeyboardMonitorService)
        {
            _faceDetection = faceDetection;
            _faceRecognition = faceRecognition;
            _captureService = captureService;
            _mouseKeyboardMonitorService = mouseKeyboardMonitorService;

            InitStateMachine();
        }

        private void OnKeyboardAction(int obj)
        {
            Debug.WriteLine("Keyboard action");
        }

        private void OnMouseAction(int obj)
        {
            Debug.WriteLine("Mouse action");
        }


        public void Start()
        {
            _sm.Next(Triggers.Start);
        }

        private Task<(bool faceDetected, bool faceRecognized)> RecognizeFace()
        {
            return Task.Factory.StartNew<(bool faceDetected, bool faceRecognized)>(() =>
            {
                bool faceDetected, faceRecognized;

                using var frame = _captureService.CaptureSingleFrame();
                var rects = _faceDetection.DetectFrontalThenProfileFaces(frame);
                if (rects.Length == 0)
                {
                    faceDetected = faceRecognized = false;
                }
                else
                {
                    faceDetected = faceRecognized = true;
                }

                Log.Logger.Debug($"Face detected: {faceDetected} Face recognized: {faceRecognized}");

                return (faceDetected, faceRecognized);
            });
        }
    }
}