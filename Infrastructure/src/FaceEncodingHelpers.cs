using FaceRecognitionDotNet;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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
