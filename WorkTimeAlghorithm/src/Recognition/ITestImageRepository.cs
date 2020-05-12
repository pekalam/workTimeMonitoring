using Domain.User;
using System;
using System.Collections.Generic;

namespace WMAlghorithm
{
    public interface ITestImageRepository
    {
        int Count(User user);
        IReadOnlyList<TestImage> GetAll(User user);
        IReadOnlyList<TestImage> GetReferenceImages(User user);
        IReadOnlyList<TestImage> GetMostRecentImages(User user, DateTime startDate, int maxCount);
        TestImage Add(TestImage img);
        void Remove(TestImage img);
        void Clear(User user);
    }
}