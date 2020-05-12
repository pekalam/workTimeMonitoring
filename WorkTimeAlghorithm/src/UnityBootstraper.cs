using Unity;
using WMAlghorithm.Services;

namespace WMAlghorithm
{
    public class UnityBootstraper
    {
        public static void Init(IUnityContainer container)
        {
            container.RegisterType<ICaptureService, CaptureService>();
            container.RegisterType<IHeadPositionService, HeadPositionService>();
            container.RegisterType<IHcFaceDetection, HcFaceDetection>();
            container.RegisterType<IDnFaceRecognition, DnFaceRecognition>();
            container.RegisterSingleton<IMouseKeyboardMonitorService, MouseKeyboardMonitorService>();
            container.RegisterType<IMouseKeyboardMonitorService, MouseKeyboardMonitorService>();

            container.RegisterSingleton<AlgorithmService>();
        }
    }
}
