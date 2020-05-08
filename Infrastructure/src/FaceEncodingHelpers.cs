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
    internal static class FaceEncodingHelpers
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
}
