using System;
using System.Collections.Generic;
using OpenCvSharp;

namespace Infrastructure.WorkTime
{


    public partial class WMonitorAlghorithm
    {
        private readonly ITestImageRepository _testImageRepository;
        private readonly DnFaceRecognition _dnFaceRecognition;
        private readonly HcFaceDetection _faceDetection;
        private readonly LbphFaceRecognition _lbphFaceRecognition;
        private readonly CaptureService _captureService = new CaptureService();
        private readonly InitFaceService _initFaceService;

        public WMonitorAlghorithm()
        {
            _testImageRepository = new DefaultTestImageRepository();
            _dnFaceRecognition = new DnFaceRecognition(_testImageRepository);
            _lbphFaceRecognition = new LbphFaceRecognition(_testImageRepository);
            _faceDetection = new HcFaceDetection();
            // _initFaceService = new InitFaceService(_testImageRepository, _dnFaceRecognition, _faceDetection,
            //     _lbphFaceRecognition, _captureService, new HeadPositionService());
            InitStateMachine();
        }


        public void Start(IProgress<InitFaceProgressArgs> progress)
        {
            _initFaceService.InitFaceProgress = progress;
            _sm.Next(Triggers.Start);
        }
    }

}