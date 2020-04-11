using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.WorkTime
{
    public interface ITestImageRepository
    {
        TestImage GetRandomImage();
        int Count { get; }
        IReadOnlyList<TestImage> GetAll();
        void Add(TestImage img);
        void Remove(TestImage img);
        void Clear();
    }

    public class DefaultTestImageRepository : ITestImageRepository
    {
        private readonly List<TestImage> _imgs = new List<TestImage>();

        public TestImage GetRandomImage()
        {
            if (_imgs.Count == 0)
            {
                throw new Exception("Empty testImgStore");
            }

            var rnd = new Random();
            var img = _imgs[rnd.Next(_imgs.Count)];
            return img;
        }

        public int Count => _imgs.Count;
        public IReadOnlyList<TestImage> GetAll() => _imgs;

        private void ValidateWithPrevious(TestImage img)
        {
            if (img.FaceGrayscale.Img.Rows != _imgs.Last().FaceGrayscale.Img.Rows)
            {
                throw new Exception("Invalid rows count in face img");
            }
            if (img.FaceGrayscale.Img.Cols != _imgs.Last().FaceGrayscale.Img.Cols)
            {
                throw new Exception("Invalid cols count in face img");
            }
            if (img.FaceColor.Img.Rows != _imgs.Last().FaceColor.Img.Rows)
            {
                throw new Exception("Invalid rows count in face color img");
            }
            if (img.FaceColor.Img.Cols != _imgs.Last().FaceColor.Img.Cols)
            {
                throw new Exception("Invalid cols count in face color img");
            }
        }

        public void Add(TestImage img)
        {
            if (img == null)
            {
                throw new NullReferenceException("Null testImage");
            }
            if (_imgs.Count > 0)
            {
                ValidateWithPrevious(img);
            }

            _imgs.Add(img);
        }

        public void Remove(TestImage img)
        {
            if (img == null)
            {
                throw new NullReferenceException("Null testImage");
            }
            if (!_imgs.Remove(img))
            {
                throw new Exception("");
            }
        }

        public void Clear()
        {
            _imgs.Clear();
        }
    }
}