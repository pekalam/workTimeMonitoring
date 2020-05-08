using System;
using System.Collections.Generic;
using System.Text;
using Domain.Services;
using Unity;

namespace WorkTimeAlghorithm
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
        }
    }
}
