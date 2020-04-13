using System;
using System.IO;
using System.Linq;
using OpenCvSharp;

namespace UnitTests
{
    class ImageTestUtils
    {
        public ImageTestUtils()
        {
            TestImgDir = Environment.GetEnvironmentVariable(nameof(TestImgDir));
            if (!Directory.Exists(TestImgDir))
            {
                throw new Exception($"Directory {TestImgDir} doesn't exist");
            }
        }

        public string TestImgDir { get; }

        public (Rect faceRect, Mat faceImg) GetFaceImg(string imgName)
        {
            var fileName = Directory.EnumerateFiles(TestImgDir).FirstOrDefault(n => n.Contains(imgName));
            if (fileName == null)
            {
                throw new Exception($"Invalid imgName: {imgName}");
            }

            var parts = fileName.Split('_', StringSplitOptions.RemoveEmptyEntries);
            parts[^1] = parts[^1].Split('.', StringSplitOptions.RemoveEmptyEntries).First();

            if (parts.Length != 5)
            {
                throw new Exception($"Invalid filename: {fileName}");
            }

            var rect = new Rect(parts[1].AsInt(), parts[2].AsInt(), parts[3].AsInt(), parts[4].AsInt());
            var faceImg = Cv2.ImRead(fileName);

            return (rect, faceImg);
        }

        public Mat GetImage(string imgName)
        {
            var fileName = Directory.EnumerateFiles(TestImgDir).FirstOrDefault(n => n.Contains(imgName));
            if (fileName == null)
            {
                throw new Exception($"Invalid imgName: {imgName}");
            }

            return Cv2.ImRead(fileName);
        }
    }
}