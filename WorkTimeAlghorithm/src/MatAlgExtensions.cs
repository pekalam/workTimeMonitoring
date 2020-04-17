using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using FaceRecognitionDotNet;
using OpenCvSharp;

namespace WorkTimeAlghorithm.src
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
