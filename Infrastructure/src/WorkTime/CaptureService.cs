using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using OpenCvSharp;

namespace Infrastructure.WorkTime
{
    public interface ICaptureService
    {
        Mat CaptureSingleFrame();
        IAsyncEnumerable<Mat> CaptureFrames([EnumeratorCancellation] CancellationToken ct);
    }

    public class CaptureService : ICaptureService
    {
        public Mat CaptureSingleFrame()
        {
            VideoCapture cap = new VideoCapture(0);
            var mat = cap.RetrieveMat();
            cap.Release();

            if (mat.Empty() || mat.Width <= 0 || mat.Height <= 0)
            {
                throw new Exception("Invalid photo");
            }

            return mat;
        }

        public async IAsyncEnumerable<Mat> CaptureFrames([EnumeratorCancellation] CancellationToken ct)
        {
            //todo RX?
            VideoCapture _cap = new VideoCapture(0);

            while (!ct.IsCancellationRequested)
            {
                //todo
                try
                {
                    await Task.Delay(34, ct).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                var frame = _cap.RetrieveMat();
                yield return frame;
            }

            _cap.Release();
        }
    }
}