using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenCvSharp;

namespace WindowUI.FaceInitialization
{
    /// <summary>
    /// Interaction logic for FaceInitializationView
    /// </summary>
    public partial class FaceInitializationView : UserControl
    {
        private int orgWidth = 0;
        private int orgHeight = 0;

        public FaceInitializationView()
        {
            InitializeComponent();

            (DataContext as FaceInitializationViewModel).OnFrameChanged = OnFrameChanged;
            (DataContext as FaceInitializationViewModel).OnFaceDetected = OnFaceDetected;
        }

        private void OnFaceDetected(Rect obj)
        {
            var sw = (image.ActualWidth / orgWidth);
            var sh = (image.ActualHeight / orgHeight);


            loadingRect.Width = (int)(obj.Width * sw);
            loadingRect.Height = (int)(obj.Height * sh);


            faceRect.Width = loadingRect.Width;
            faceRect.Height = loadingRect.Height;
            

            faceRect.SetValue(Canvas.TopProperty, (double)obj.Location.Y * sh);
            faceRect.SetValue(Canvas.LeftProperty, (double)obj.Location.X * sw);
            loadingRect.SetValue(Canvas.TopProperty, (double)obj.Location.Y * sh);
            loadingRect.SetValue(Canvas.LeftProperty, (double)obj.Location.X * sw);
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
