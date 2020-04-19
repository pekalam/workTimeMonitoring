using System;
using System.Runtime.CompilerServices;
using Domain.User;
using FaceRecognitionDotNet;
using OpenCvSharp;
using Unity;
using WorkTimeAlghorithm.StateMachine;

[assembly: InternalsVisibleTo("Infrastructure.Tests")]
namespace WorkTimeAlghorithm
{
    internal static class TestImageBuilderFactory
    {
        public static Func<TestImageBuilder> Create = () => new TestImageBuilder();
    }

    public class TestImageBuilder
    {
        public Rect FaceLocation { get; private set; }
        public Mat Img { get; private set; }
        public HeadRotation Rotation { get; private set; }
        public DateTime DateCreated { get; private set; }
        public FaceEncodingData FaceEncoding { get; private set; }
        public bool IsReferenceImg { get; private set; }
        public User User { get; private set; }
        
        public TestImageBuilder AddFaceLocation(Rect faceLocation)
        {
            FaceLocation = faceLocation;
            return this;
        }

        public TestImageBuilder AddImg(Mat img)
        {
            Img = img;
            return this;
        }

        public TestImageBuilder AddDateCreated(DateTime dateCreated)
        {
            DateCreated = dateCreated;
            return this;
        }

        public TestImageBuilder AddHeadRotation(HeadRotation headRotation)
        {
            Rotation = headRotation;
            return this;
        }

        public TestImageBuilder AddFaceEncoding(FaceEncodingData faceEncoding)
        {
            FaceEncoding = faceEncoding;
            return this;
        }

        public TestImageBuilder SetIsReferenceImg(bool isReferenceImg)
        {
            IsReferenceImg = isReferenceImg;
            return this;
        }

        public TestImageBuilder SetUser(User user)
        {
            User = user;
            return this;
        }

        public virtual TestImage Build()
        {
            return new TestImage(FaceEncoding, FaceLocation, Img, Rotation, DateCreated, IsReferenceImg, User.UserId);
        }
    }

    public class FaceEncodingData
    {
        internal static readonly FaceEncodingData ValidFaceEncodingData = new FaceEncodingData();

        public FaceEncoding Value { get; }

        //Test ctor
        private FaceEncodingData() { }

        public FaceEncodingData(FaceEncoding value)
        {
            if (value == null)
            {
                throw new NullReferenceException("Null faceEncoding");
            }
            if (value.IsDisposed || value.Size <= 0)
            {
                throw new ArgumentException($"Invalid faceEncoding");
            }
            Value = value;
        }
    }

    public static class TestImageDeserializationHelper
    {
        public static void SetInternalFields(long? id, TestImage img)
        {
            img.Id = id;
        }
    }

    public class TestImage
    {
        public long? Id { get; internal set; }
        public FaceEncodingData FaceEncoding { get; private set; }
        public Rect FaceLocation { get; private set; }
        public Mat Img { get; private set; }
        public HeadRotation HorizontalHeadRotation { get; private set; }
        public DateTime DateCreated { get; private set; }
        public bool IsReferenceImg { get; private set; }
        public long UserId { get; private set; }


        internal TestImage()
        {
            
        }

        public TestImage(FaceEncodingData faceEncoding, Rect faceLocation, Mat img, HeadRotation horizontalHeadRotation, DateTime dateCreated, bool isReferenceImg, long userId)
        {
            if (faceLocation.Width <= 0 || faceLocation.Height <= 0)
            {
                throw new ArgumentException($"Invalid faceLocation");
            }

            if (img.Empty())
            {
                throw new ArgumentException("Empty img");
            }

            if (img.Rows <= 0 || img.Cols <= 0)
            {
                throw new ArgumentException("Invalid img size");
            }

            if (horizontalHeadRotation == HeadRotation.Unknown)
            {
                throw new ArgumentException("Unknown headRotation");
            }

            FaceEncoding = faceEncoding ?? throw new ArgumentException("Null faceEncodingData");
            FaceLocation = faceLocation;
            Img = img;
            HorizontalHeadRotation = horizontalHeadRotation;
            DateCreated = dateCreated;
            IsReferenceImg = isReferenceImg;
            UserId = userId;
        }

        //Test ctor
        internal TestImage(Rect faceLocation, Mat img, HeadRotation horizontalHeadRotation, DateTime dateCreated, bool isReferenceImg, long userId)
        {
            FaceLocation = faceLocation;
            Img = img;
            HorizontalHeadRotation = horizontalHeadRotation;
            DateCreated = dateCreated;
            IsReferenceImg = isReferenceImg;
            UserId = userId;
        }
    }
}