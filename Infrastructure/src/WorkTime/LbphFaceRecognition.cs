using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenCvSharp;
using OpenCvSharp.Face;

namespace Infrastructure.WorkTime
{
    public interface ILbphFaceRecognition
    {
        bool Trained { get; }
        double ConfidenceThreshold { get; }
        void Train(FaceImg grayscaleFaceImg, int label = 1);
        void Update(Mat photo);
        bool RecognizeFace(Mat photo);
        void Reset();
    }

    public class LbphFaceRecognition : ILbphFaceRecognition
    {
        private const double MinConfidenceThreshold = 1000;

        private readonly LBPHFaceRecognizer _recognizer;
        private readonly ITestImageRepository _testImageRepository;

        private double _confidenceThreshold;
        private int _trainingSetSize = 0;
        private double _stdSum;

        public LbphFaceRecognition(ITestImageRepository testImageRepository)
        {
            _recognizer = LBPHFaceRecognizer.Create(gridX: 16, gridY: 16);
            _testImageRepository = testImageRepository;
        }

        public bool Trained => _trainingSetSize > 0;
        public double ConfidenceThreshold => _confidenceThreshold;

        private void ResetStateVars()
        {
            _trainingSetSize = 0;
            _stdSum = _confidenceThreshold = 0;
        }

        private void CheckIsTrained()
        {
            if (!Trained)
            {
                throw new Exception("Not trained");
            }
        }

        private double CheckConfidenceAfterTraining()
        {
            var testImage = _testImageRepository.GetRandomImage();
            _recognizer.Predict(testImage.FaceGrayscale.Img, out var label, out var confidence);

            Debug.WriteLine($"Checked confidende: {confidence}");

            if (label == 0)
            {
                ResetStateVars();
                throw new Exception("Unknown face");
            }

            if (confidence > MinConfidenceThreshold)
            {
                ResetStateVars();
                throw new Exception($"Too high confidence: {confidence}");
            }

            return confidence;
        }

        public void Train(FaceImg grayscaleFaceImg, int label = 1)
        {
            if (_trainingSetSize > 0)
            {
                _recognizer.Predict(grayscaleFaceImg.Img, out _, out var confidence);
                if (confidence > _confidenceThreshold)
                {
                    Debug.WriteLine($"ignoring {confidence}");
                    return;
                }
            }

            _recognizer.Train(new[] { grayscaleFaceImg.Img }, new[] { label });

            if (label == 1)
            {
                _trainingSetSize++;
                if (_testImageRepository.Count > 0)
                {
                    var confidence = CheckConfidenceAfterTraining();
                    _stdSum += confidence;
                }
                else
                {
                    _stdSum = MinConfidenceThreshold;
                    //_stdSum = 0;
                }


                _confidenceThreshold = _stdSum / _trainingSetSize;
            }

        }



        public void Update(Mat photo)
        {
            CheckIsTrained();

            _recognizer.Update(new []{photo}, new []{1});
            _recognizer.Predict(photo, out var label, out var confidence);
            _trainingSetSize++;
            _stdSum += confidence;

            if (_stdSum / _trainingSetSize < _confidenceThreshold)
            {
                _confidenceThreshold = _stdSum / _trainingSetSize;
            }
        }

        public bool RecognizeFace(Mat photo)
        {
            CheckIsTrained();

            _recognizer.Predict(photo, out var label, out var confidence);
            Debug.WriteLine($"RecognizeFace confidende: {confidence}");
            return confidence <= _confidenceThreshold;
        }

        public double RecognizeFace2(Mat photo)
        {
            CheckIsTrained();

            _recognizer.Predict(photo, out var label, out var confidence);
            return confidence;
        }

        public void Reset() => ResetStateVars();
    }
}