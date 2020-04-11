using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using FaceRecognitionDotNet;
using Infrastructure;
using Infrastructure.WorkTime;
using OpenCvSharp;
using Image = FaceRecognitionDotNet.Image;
using Size = OpenCvSharp.Size;
using Window = System.Windows.Window;

namespace LpbhFaceRecognitionTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CaptureService _cap = new CaptureService();
        private ITestImageRepository _repo = new DefaultTestImageRepository();
        private LbphFaceRecognition _recognition;
        private HcFaceDetection _faceDetection = new HcFaceDetection();

        private FaceImg _currentFace;
        private FaceImg _photo;
        private Mat _currentFrame;

        private int _frames = 0;
        private int _trained = 0;
        private bool _stop = true;

        public MainWindow()
        {
            _recognition = new LbphFaceRecognition(_repo);
            InitializeComponent();
        }

        protected override async void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            await foreach (var frame in _cap.CaptureFrames(CancellationToken.None))
            {
                _currentFrame = frame.Clone();
                var (rects, faces) = _faceDetection.DetectFrontalThenProfileFaces(frame);

                if (rects.Length == 1)
                {
                    _currentFace = FaceImg.CreateGrayscale(faces.First());
                    Cv2.Rectangle(frame, rects.First(), Scalar.Green);

                    _frames++;
                    if (_frames == 5 && !_stop)
                    {
                        photoBtn_Click(null, null);
                        trainBtn_Click(null, null);
                        trained.Text = _trained.ToString();
                        _frames = 0;
                    }
                }


                image.Source = frame.ToBitmapImage();
            }
        }

        private Image LoadImage(Mat photo)
        {
            var bytes = new byte[photo.Rows * photo.Cols * photo.ElemSize()];
            Marshal.Copy(photo.Data, bytes, 0, bytes.Length);

            var img = FaceRecognition.LoadImage(bytes, photo.Rows, photo.Cols, photo.ElemSize());
            return img;
        }

        private Mat Preprocces(Mat face)
        {
            var landkarms = FaceRecognitionModel.Model.FaceLandmark(LoadImage(face), model: PredictorModel.Large)
                .ToList();

            var mix = landkarms.SelectMany(v => v.Values).SelectMany(v => v).Min(v => v.X);
            var max = landkarms.SelectMany(v => v.Values).SelectMany(v => v).Max(v => v.X);
            var miy = landkarms.SelectMany(v => v.Values).SelectMany(v => v).Min(v => v.Y);
            var may = landkarms.SelectMany(v => v.Values).SelectMany(v => v).Max(v => v.Y);


            return face.SubMat(miy, may, mix, max);
        }

        private void predictBtn_Click(object sender, RoutedEventArgs e)
        {
            distance.Text = _recognition.RecognizeFace(_photo.Img).ToString();
        }

        private void trainBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_trained >= 0)
            {
                _recognition.Train(_currentFace);
                if (_repo.Count < 3)
                    _repo.Add(new TestImage(_currentFace, _currentFace));
            }
            else
            {
                _recognition.Update(_currentFace.Img);
            }
            threshold.Text = _recognition.ConfidenceThreshold.ToString("F3");

            _trained++;
        }

        private void photoBtn_Click(object sender, RoutedEventArgs e)
        {
            var f= Preprocces(_currentFrame);
            Cv2.Resize(f,f, new Size(FaceImg.Width, FaceImg.Height));
            Cv2.CvtColor(f, f, ColorConversionCodes.BGR2GRAY);

            var k = Cv2.GetGaborKernel(new Size(40, 40), 5, 2, 1.0, 1, Math.PI/2.0, MatType.CV_32F);
            Cv2.Filter2D(f,f,MatType.CV_8U, k);
            Cv2.ImShow("x", f);
            // _currentFace = new FaceImg(f);
            // _photo = _currentFace;
            // preview.Source = _photo.Img.ToBitmapImage();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {




            _stop = !_stop;
            _frames = 0;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var imgs = new string[0];
            var labels = new int[0];

            for (int i = 1; i <= 40; i++)
            {
                for (int j = 1; j <= 10; j++)
                {
                    imgs = imgs.Append($"D:\\att_faces\\s{i}\\{j}.pgm").ToArray();
                    labels = labels.Append(0).ToArray();
                }
            }

            var faceImg = imgs.Select(p => Cv2.ImRead(p)).Select(m => 
            {
                Cv2.Resize(m, m, new Size(FaceImg.Width, FaceImg.Height));
                Cv2.CvtColor(m, m, ColorConversionCodes.BGR2GRAY);
                return new FaceImg(m);
            });

            foreach (var img in faceImg)
            {
                _recognition.Train(img, 0);
            }
        }
    }
}