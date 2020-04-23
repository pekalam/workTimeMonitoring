using System;

namespace Domain.WorkTimeAggregate.Events
{
    public class FaceRecognitionFailure : Event
    {
        public FaceRecognitionFailure(long aggregateId, DateTime date, bool faceRecognized, bool faceDetected, DateTime endDate, long lengthMs) : base(aggregateId, date, EventName.FaceRecognitionFailure)
        {
            if (faceDetected && faceRecognized)
            {
                throw new ArgumentException("Invalid FaceRecognitionFailure ctor params");
            }
            FaceRecognized = faceRecognized;
            FaceDetected = faceDetected;
            EndDate = endDate;
            LengthMs = lengthMs;
        }

        public bool FaceRecognized { get; }
        public bool FaceDetected { get; }
        public DateTime EndDate { get; }
        public long LengthMs { get; }
    }
}