using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("WorkTimeAlghorithm.UnitTests")]
namespace WMAlghorithm
{
    public interface ICaptureService
    {
        bool IsCapturing { get; }
        Mat CaptureSingleFrame();
        IAsyncEnumerable<Mat> CaptureFrames(CancellationToken ct);
    }

    public class CaptureService : ICaptureService
    {
        private static bool _isCapturing;

        public bool IsCapturing => _isCapturing;

        private void ValidateMat(Mat mat)
        {
            if (mat.Empty() || mat.Width <= 0 || mat.Height <= 0)
            {
                throw new Exception("Invalid photo");
            }

        }

        public Mat CaptureSingleFrame()
        {
            VideoCapture cap = new VideoCapture(0);
            var mat = cap.RetrieveMat();
            cap.Release();

            ValidateMat(mat);

            return mat;
        }

        public async IAsyncEnumerable<Mat> CaptureFrames([EnumeratorCancellation] CancellationToken ct)
        {
            _isCapturing = true;
            VideoCapture cap = new VideoCapture(0);
            
            while (!ct.IsCancellationRequested)
            {
                //todo
                try
                {
                    await Task.Delay(34, ct);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                var frame = cap.RetrieveMat();
                if (frame != null)
                {
                    ValidateMat(frame);
                    yield return frame;
                }
            }

            cap.Release();
            _isCapturing = false;
        }
    }
}