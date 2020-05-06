using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;

[assembly: InternalsVisibleTo("WorkTimeAlghorithm.UnitTests")]
namespace WorkTimeAlghorithm
{
    public interface ICaptureService
    {
        Mat CaptureSingleFrame();
        IAsyncEnumerable<Mat> CaptureFrames(CancellationToken ct);
    }

    public class CaptureService : ICaptureService
    {
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
        }
    }
}