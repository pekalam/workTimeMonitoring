using System;
using System.Reflection.Metadata;
using System.Runtime.Serialization;
using FaceRecognitionDotNet;

namespace Infrastructure.Db
{
    internal class DbTestImage
    {
        public int? Id { get; set; }
        public int FaceLocation_x { get; set; }
        public int FaceLocation_y { get; set; }
        public int FaceLocation_width { get; set; }
        public int FaceLocation_height { get; set; }
        public byte[] Img { get; set; }
        public int HorizontalHeadRotation { get; set; }
        public DateTime DateCreated { get; set; }
        public byte[] FaceEncoding { get; set; }
        public bool IsReferenceImg { get; set; }
        public long UserId { get; set; }
    }
}