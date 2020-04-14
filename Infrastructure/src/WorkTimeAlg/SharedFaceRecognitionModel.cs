using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FaceRecognitionDotNet;

namespace Infrastructure.WorkTimeAlg
{
    public static class SharedFaceRecognitionModel
    {
        private static object _lck = new object();
        private static readonly Task<FaceRecognition> _loadTask;

        static SharedFaceRecognitionModel()
        {
            _loadTask = Task.Factory.StartNew<FaceRecognition>(() => FaceRecognition.Create("."), TaskCreationOptions.LongRunning);
        }

        public static List<FaceEncoding> FaceEncodingsSync(Image image,
            IEnumerable<Location> knownFaceLocation = null,
            int numJitters = 1,
            PredictorModel model = PredictorModel.Small)
        {
            lock (_lck)
            {
                return _loadTask.Result.FaceEncodings(image, knownFaceLocation, numJitters, model).ToList();
            }
        }

        public static FaceRecognition Model => _loadTask.Result;
    }
}