using System;
using System.Collections.Generic;
using Infrastructure.WorkTimeAlg;

namespace Infrastructure.Repositories
{
    public interface ITestImageRepository
    {
        int Count { get; }
        IReadOnlyList<TestImage> GetAll();
        IReadOnlyList<TestImage> GetReferenceImages();
        IReadOnlyList<TestImage> GetMostRecentImages(DateTime startDate, int maxCount);
        void Add(TestImage img);
        void Remove(TestImage img);
        void Clear();
    }
}