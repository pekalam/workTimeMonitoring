using System;

namespace Domain.WorkTimeAggregate.Events
{
    public class FaceRecognitionFailure : Event
    {
        public FaceRecognitionFailure(long aggregateId, DateTime date, bool faceRecognized, bool faceDetected) : base(aggregateId, date, EventName.FaceRecognitionFailure)
        {
            if (faceDetected && faceRecognized)
            {
                throw new ArgumentException("Invalid FaceRecognitionFailure ctor params");
            }
            FaceRecognized = faceRecognized;
            FaceDetected = faceDetected;
        }

        public bool FaceRecognized { get; }
        public bool FaceDetected { get; }
    }
}