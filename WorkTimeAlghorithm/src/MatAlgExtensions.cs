using FaceRecognitionDotNet;
using OpenCvSharp;
using System.Runtime.InteropServices;

namespace WMAlghorithm
{
    public static class MatAlgExtensions
    {
        public static Image ToImage(this Mat photo)
        {
            var bytes = new byte[photo.Rows * photo.Cols * photo.ElemSize()];
            Marshal.Copy(photo.Data, bytes, 0, bytes.Length);

            var img = FaceRecognition.LoadImage(bytes, photo.Rows, photo.Cols, photo.ElemSize());
            return img;
        }
    }
}
