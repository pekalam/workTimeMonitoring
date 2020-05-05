using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using Prism.Mvvm;
using Prism.Regions;

namespace WindowUI.TriggerRecognition
{
    public class TriggerRecognitionViewModel : BindableBase, INavigationAware
    {
        private readonly ITriggerRecognitionController _controller;

        public event Action<BitmapSource> OnFrameChanged;
        public event Action<Rect> OnFaceDetected;
        public event Action OnNoFaceDetected;

        public TriggerRecognitionViewModel(ITriggerRecognitionController controller)
        {
            _controller = controller;
            Retry = controller.Retry;
        }

        public ICommand Retry { get; }

        public Brush FaceRectBrush { get; set; } = Brushes.Blue;

        public void CallOnFrameChanged(BitmapSource bmp)
        {
            OnFrameChanged?.Invoke(bmp);
        }

        public void CallOnFaceDetected(Rect rect)
        {
            OnFaceDetected?.Invoke(rect);
        }

        public void CallOnNoFaceDetected()
        {
            OnNoFaceDetected?.Invoke();
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            await _controller.Init(this);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }

    public interface ITriggerRecognitionController
    {
        ICommand Retry { get; }
        Task Init(TriggerRecognitionViewModel vm);
    }
}
