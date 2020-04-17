using System;
using System.Collections.Generic;

namespace WorkTimeAlghorithm
{
    public interface ITestImageRepository
    {
        int Count { get; }
        IReadOnlyList<TestImage> GetAll();
        IReadOnlyList<TestImage> GetReferenceImages();
        IReadOnlyList<TestImage> GetMostRecentImages(DateTime startDate, int maxCount);
        TestImage Add(TestImage img);
        void Remove(TestImage img);
        void Clear();
    }
}