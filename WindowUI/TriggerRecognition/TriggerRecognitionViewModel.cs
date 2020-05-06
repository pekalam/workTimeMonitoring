using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Prism.Mvvm;
using Prism.Regions;
using Rect = OpenCvSharp.Rect;

namespace WindowUI.TriggerRecognition
{
    public class TriggerRecognitionViewModel : BindableBase, INavigationAware
    {
        private readonly ITriggerRecognitionController _controller;
        private Visibility _recognizedOverlayVisibility = Visibility.Hidden;
        private bool? _faceRecognized;
        private bool _loading;

        public event Action<BitmapSource> OnFrameChanged;
        public event Action<Rect> OnFaceDetected;
        public event Action OnNoFaceDetected;

        public TriggerRecognitionViewModel(ITriggerRecognitionController controller)
        {
            _controller = controller;
            Retry = controller.Retry;
        }

        public ICommand Retry { get; }

        public bool? FaceRecognized
        {
            get => _faceRecognized;
            set =>  SetProperty(ref _faceRecognized, value);
        }

        public Visibility RecognizedOverlayVisibility
        {
            get => _recognizedOverlayVisibility;
            set => SetProperty(ref _recognizedOverlayVisibility, value);
        }

        public bool Loading
        {
            get => _loading;
            set => SetProperty(ref _loading, value);
        }

        public void ShowRecognitionFailure()
        {
            RecognizedOverlayVisibility = Visibility.Visible;
            FaceRecognized = false;
        }

        public void ShowRecognitionSuccess()
        {
            RecognizedOverlayVisibility = Visibility.Visible;
            FaceRecognized = true;
        }

        public void ResetRecognition()
        {
            RecognizedOverlayVisibility = Visibility.Hidden;
            FaceRecognized = null;
        }

        public void ShowLoading()
        {
            RecognizedOverlayVisibility = Visibility.Visible;
            Loading = true;
        }

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
            await _controller.Init(this, (bool) navigationContext.Parameters["WindowOpened"], navigationContext.Parameters["PreviousView"]);
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
        Task Init(TriggerRecognitionViewModel vm, bool windowOpened, object previousView);
    }
}