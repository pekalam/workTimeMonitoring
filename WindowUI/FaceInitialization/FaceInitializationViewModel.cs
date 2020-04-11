using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using OpenCvSharp;
using Prism.Mvvm;
using Prism.Regions;

namespace WindowUI.FaceInitialization
{
    public interface IFaceInitializationController
    {
        void Init(FaceInitializationViewModel vm);
        ICommand StepInfoContinueClick { get; }
        ICommand StepInfoRetryClick { get; }
    }

    public class FaceInitializationViewModel : BindableBase, INavigationAware
    {
        private int _progress;
        private IFaceInitializationController _controller;

        public Action<BitmapSource> OnFrameChanged;
        public Action<Rect> OnFaceDetected;
        private Brush _stepInfoPanelBrush = Brushes.Green;
        private Brush _faceRectBrush = Brushes.Blue;
        private bool _loadingOverlayVisible;
        private string _loadingOverlayText;
        private bool _stepInfoPanelVisible;
        private string _stepInfoPanelText;
        private bool _stepInfoContinueVisible;
        private bool _stepInfoRetryVisible;
        private bool _photoPreviewVisible;
        private ImageSource _photo2;
        private ImageSource _photo1;
        private ImageSource _photo3;

        public FaceInitializationViewModel(IFaceInitializationController controller)
        {
            _controller = controller;
            StepInfoContinueClick = _controller.StepInfoContinueClick;
            StepInfoRetryClick = _controller.StepInfoRetryClick;
        }

        public ICommand StepInfoContinueClick { get; }
        public ICommand StepInfoRetryClick { get; }

        public int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        public Brush FaceRectBrush
        {
            get => _faceRectBrush;
            set => SetProperty(ref _faceRectBrush, value);
        }

        public bool LoadingOverlayVisible
        {
            get => _loadingOverlayVisible;
            set => SetProperty(ref _loadingOverlayVisible, value);
        }

        public string LoadingOverlayText
        {
            get => _loadingOverlayText;
            set => SetProperty(ref _loadingOverlayText, value);
        }

        public Brush StepInfoPanelBrush
        {
            get => _stepInfoPanelBrush;
            set => SetProperty(ref _stepInfoPanelBrush, value);
        }

        public bool StepInfoPanelVisible
        {
            get => _stepInfoPanelVisible;
            set => SetProperty(ref _stepInfoPanelVisible, value);
        }

        public string StepInfoPanelText
        {
            get => _stepInfoPanelText;
            set => SetProperty(ref _stepInfoPanelText, value);
        }

        public bool StepInfoContinueVisible
        {
            get => _stepInfoContinueVisible;
            set => SetProperty(ref _stepInfoContinueVisible, value);
        }

        public bool StepInfoRetryVisible
        {
            get => _stepInfoRetryVisible;
            set => SetProperty(ref _stepInfoRetryVisible, value);
        }

        public bool PhotoPreviewVisible
        {
            get => _photoPreviewVisible;
            set => SetProperty(ref _photoPreviewVisible, value);
        }

        public ImageSource Photo1
        {
            get => _photo1;
            set => SetProperty(ref _photo1, value);
        }

        public ImageSource Photo2
        {
            get => _photo2;
            set => SetProperty(ref _photo2, value);
        }

        public ImageSource Photo3
        {
            get => _photo3;
            set => SetProperty(ref _photo3, value);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _controller.Init(this);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
