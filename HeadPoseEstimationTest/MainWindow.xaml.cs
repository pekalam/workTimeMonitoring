using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HeadPoseEstimationTest.Annotations;
using Infrastructure;
using Infrastructure.WorkTime;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Brush = System.Drawing.Brush;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;
using Window = System.Windows.Window;

namespace HeadPoseEstimationTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private CaptureService _cap = new CaptureService();
        private HcFaceDetection _faceDetection = new HcFaceDetection();
        private HeadPositionService _headPosition = new HeadPositionService();
        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public bool Started => _cts != null && !_cts.Token.IsCancellationRequested;
        public bool Stopped => _cts != null && _cts.Token.IsCancellationRequested;

        public string CamWidth
        {
            get => _camWidth.ToString();
            set
            {
                _camWidth = Convert.ToInt32(value);
                OnPropertyChanged(nameof(CamWidth));
            }
        }

        public string CamHeight
        {
            get => _camHeight.ToString();
            set
            {
                _camHeight = Convert.ToInt32(value);
                OnPropertyChanged(nameof(CamHeight));
            }
        }

        public string RotationState
        {
            get => _rotationState;
            set
            {
                _rotationState = value; 
                OnPropertyChanged(nameof(RotationState));
            }
        }

        public string RotationStateBrush
        {
            get => _rotationStateBrush;
            set
            {
                _rotationStateBrush = value;
                OnPropertyChanged(nameof(RotationStateBrush));
            }
        }

        private int _camWidth = 0;
        private int _camHeight = 0;
        private string _rotationState;
        private string _rotationStateBrush = "Green";

        protected override async void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            await Start();
        }

        private async Task Start()
        {
            _cts = new CancellationTokenSource();
            OnPropertyChanged(nameof(Started));
            OnPropertyChanged(nameof(Stopped));
            await foreach (var frame in _cap.CaptureFrames(_cts.Token))
            {
                if (_camWidth != 0 && _camHeight != 0)
                {
                    Cv2.Resize(frame, frame, new Size(_camWidth, _camHeight));
                }

                var (rects, faces) = _faceDetection.DetectFrontalThenProfileFaces(frame);

                int i = 0;
                foreach (var rect in rects)
                {
                    i++;
                    Cv2.Rectangle(frame, rect, Scalar.Blue);

                    var pos = _headPosition.GetHeadPosition(frame, rect);
                    Cv2.PutText(frame, $"face{i}({rect.Location.X}, {rect.Location.Y}) h:{pos.hRotation} v:{pos.vRotation}", new Point(0,60 * i), HersheyFonts.HersheyComplex, 1, Scalar.Red);
                }

                if (rects.Length == 1)
                {
                    var pos = _headPosition.GetHeadPosition(frame, rects.First());
                    if (pos.vRotation == HeadRotation.Front && pos.hRotation == HeadRotation.Front)
                    {
                        RotationStateBrush = nameof(Color.Green);
                        RotationState = "Front";
                    }
                    else
                    {
                        RotationStateBrush = nameof(Color.Red);
                        RotationState = $"Invalid head pos: (h:{pos.hRotation} v:{pos.vRotation})";
                    }
                }

                image.Source = frame.ToBitmapImage();
            }
        }

        private void Stop()
        {
            _cts.Cancel();
            OnPropertyChanged(nameof(Started));
            OnPropertyChanged(nameof(Stopped));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            await Start();
        }
    }
}
