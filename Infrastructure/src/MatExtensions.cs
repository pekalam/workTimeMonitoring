using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using FaceRecognitionDotNet;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Image = FaceRecognitionDotNet.Image;

namespace Infrastructure
{


    public static class MatExtensions
    {
        private static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public static BitmapImage ToBitmapImage(this Mat mat)
        {
            var bmp = mat.ToBitmap();
            var bmpImg = BitmapToImageSource(bmp);
            return bmpImg;
        }

        public static Image ToImage(this Mat photo)
        {
            var bytes = new byte[photo.Rows * photo.Cols * photo.ElemSize()];
            Marshal.Copy(photo.Data, bytes, 0, bytes.Length);

            var img = FaceRecognition.LoadImage(bytes, photo.Rows, photo.Cols, photo.ElemSize());
            return img;
        }
    }
}
