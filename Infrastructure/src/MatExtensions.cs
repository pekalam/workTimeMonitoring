using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Documents.Serialization;
using System.Windows.Media.Imaging;
using FaceRecognitionDotNet;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Image = FaceRecognitionDotNet.Image;

namespace Infrastructure
{
    public static class FaceEncodingHelpers
    {
        public static byte[] Serialize(FaceEncoding faceEncoding)
        {
            var bf = new BinaryFormatter();
            using var stream = new MemoryStream();
            bf.Serialize(stream, faceEncoding);
            var bytes = stream.ToArray();
            return bytes;
        }

        public static FaceEncoding Deserialize(byte[] bytes)
        {
            var bf = new BinaryFormatter();
            using var stream = new MemoryStream(bytes);
            return (FaceEncoding) bf.Deserialize(stream);
        }
    }

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
    }
}
