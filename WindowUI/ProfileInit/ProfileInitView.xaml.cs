using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Rect = OpenCvSharp.Rect;

namespace WindowUI.ProfileInit
{
    /// <summary>
    /// Interaction logic for ProfileInitView
    /// </summary>
    public partial class ProfileInitView : UserControl
    {
        private int orgWidth = 0;
        private int orgHeight = 0;

        public ProfileInitView()
        {
            InitializeComponent();


            var vm = (DataContext as ProfileInitViewModel);
            vm.OnFrameChanged += OnFrameChanged;
            vm.OnFaceDetected += OnFaceDetected;
            vm.OnNoFaceDetected += OnNoFaceDetected;
        }

        private void OnNoFaceDetected()
        {

        }

        private void OnFaceDetected(Rect obj)
        {

            var sw = (image.ActualWidth / orgWidth);
            var sh = (image.ActualHeight / orgHeight);


            loadingRect.Width = (int)(obj.Width * sw);
            loadingRect.Height = (int)(obj.Height * sh);

            faceRect.Width = startStepButton.Width = loadingRect.Width;
            faceRect.Height = startStepButton.Height = loadingRect.Height;

            var faceX = obj.Location.X * sw;
            var faceY = obj.Location.Y * sh;

            faceRect.SetValue(Canvas.TopProperty, faceY);
            faceRect.SetValue(Canvas.LeftProperty, faceX);
            
            loadingRect.SetValue(Canvas.TopProperty, faceY);
            loadingRect.SetValue(Canvas.LeftProperty, faceX);

            startStepButton.SetValue(Canvas.TopProperty, faceY);
            startStepButton.SetValue(Canvas.LeftProperty, faceX);

            percentageText.SetValue(Canvas.TopProperty, faceY - percentageText.DesiredSize.Height - 4);
            percentageText.SetValue(Canvas.LeftProperty, faceX + faceRect.Width / 2 - percentageText.DesiredSize.Width/2);
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
