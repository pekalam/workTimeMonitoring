using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace UI.Common.Extensions
{
    public static class MatExtensions
    {
        private static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using MemoryStream memory = new MemoryStream();
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
            memory.Position = 0;
            BitmapImage bitmapimage = new BitmapImage();
            bitmapimage.BeginInit();
            bitmapimage.StreamSource = memory;
            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapimage.EndInit();

            return bitmapimage;
        }

        public static BitmapImage ToBitmapImage(this Mat mat)
        {
            var bmp = mat.ToBitmap();
            var bmpImg = BitmapToImageSource(bmp);
            return bmpImg;
        }
    }
}
