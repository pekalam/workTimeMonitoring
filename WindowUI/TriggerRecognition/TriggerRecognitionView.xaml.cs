using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Rect = OpenCvSharp.Rect;

namespace WindowUI.TriggerRecognition
{
    /// <summary>
    /// Interaction logic for TriggerRecognitionView
    /// </summary>
    public partial class TriggerRecognitionView : UserControl
    {
        private int orgWidth = 0;
        private int orgHeight = 0;

        public TriggerRecognitionView()
        { 
            InitializeComponent();

            var vm = (DataContext as TriggerRecognitionViewModel);
            vm.OnFrameChanged += OnFrameChanged;
            vm.OnFaceDetected += OnFaceDetected;
            vm.OnNoFaceDetected += VmOnOnNoFaceDetected;
        }

        private void VmOnOnNoFaceDetected()
        {
            faceRect.Visibility = Visibility.Hidden;
        }

        private void OnFaceDetected(Rect obj)
        {
            
            var sw = (image.ActualWidth / orgWidth);
            var sh = (image.ActualHeight / orgHeight);

            var faceX = obj.Location.X * sw;
            var faceY = obj.Location.Y * sh;

            faceRect.Visibility = Visibility.Visible;
            faceRect.Width = (int)(obj.Width * sw);
            faceRect.Height = (int)(obj.Height * sh);
            faceRect.SetValue(Canvas.TopProperty, faceY);
            faceRect.SetValue(Canvas.LeftProperty, faceX);
        }

        private void OnFrameChanged(BitmapSource obj)
        {
            orgWidth = obj.PixelWidth;
            orgHeight = obj.PixelHeight;
            image.Source = obj;
            canvas.Width = image.RenderSize.Width;
            canvas.Height = image.RenderSize.Height;
        }
    }
}
