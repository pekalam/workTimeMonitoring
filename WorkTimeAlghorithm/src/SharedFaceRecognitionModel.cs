using Domain.Services;
using FaceRecognitionDotNet;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMAlghorithm
{
    public class FaceRecognitionModelSettings
    {
        public string Location { get; set; } = ".";
    }

    public static class SharedFaceRecognitionModel
    {
        private static object _lck = new object();
        private static Task<FaceRecognition> _loadTask;



        public static void Init(IConfigurationService config)
        {
            var settings = config.Get<FaceRecognitionModelSettings>("faceRecognition");
            _loadTask = Task.Factory.StartNew<FaceRecognition>(() => FaceRecognition.Create(settings.Location),
                TaskCreationOptions.LongRunning);
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

        public static Task<FaceRecognition> LoadTask => _loadTask;
        public static FaceRecognition Model => _loadTask.Result;
    }
}