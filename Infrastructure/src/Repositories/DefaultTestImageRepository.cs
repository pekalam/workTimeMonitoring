using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.WorkTime;

namespace Infrastructure.Repositories
{
    internal class DefaultTestImageRepository : ITestImageRepository
    {
        private readonly List<TestImage> _imgs = new List<TestImage>();

        public int Count => _imgs.Count;
        public IReadOnlyList<TestImage> GetAll() => _imgs;

        public IReadOnlyList<TestImage> GetReferenceImages()
        {
            return _imgs.Where(i => i.IsReferenceImg == true).ToList();
        }

        public IReadOnlyList<TestImage> GetMostRecentImages(DateTime startDate, int maxCount)
        {
            return _imgs.Where(i => i.DateCreated >= startDate).TakeLast(maxCount).ToList();
        }

        private void ValidateWithPrevious(TestImage img)
        {
            if (img.Img.Rows != _imgs.Last().Img.Rows)
            {
                throw new Exception("Invalid rows count in face img");
            }
            if (img.Img.Cols != _imgs.Last().Img.Cols)
            {
                throw new Exception("Invalid cols count in face img");
            }
            if (img.Img.Rows != _imgs.Last().Img.Rows)
            {
                throw new Exception("Invalid rows count in face color img");
            }
            if (img.Img.Cols != _imgs.Last().Img.Cols)
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
            img.Id = _imgs.Count;
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