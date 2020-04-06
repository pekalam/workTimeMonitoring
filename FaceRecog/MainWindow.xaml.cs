using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Face;
using Size = OpenCvSharp.Size;
using Window = System.Windows.Window;

namespace FaceRecog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CascadeClassifier _face_cascade = new CascadeClassifier("haarcascade_frontalface_default.xml");
        private VideoCapture _capture = new VideoCapture(0);
        private DispatcherTimer _timer = new DispatcherTimer();
        private FaceRecognizer _recognizer;
        private BackgroundSubtractor _subtractor;

        public MainWindow()
        {
            InitializeComponent();
            _timer.Interval = TimeSpan.FromMilliseconds(1000 / 60.0f);
            _timer.Tick += TimerOnTick;
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            var imgs = new[] {"D:\\WIN_20200404_02_03_09_Pro.jpg", "D:\\cena.png", "D:\\WIN_20200404_02_03_02_Pro.jpg"};
            var labels = new[] {1, 0, 1};

            // for (int i = 1; i <= 40; i++)
            // {
            //     for (int j = 1; j <= 10; j++)
            //     {
            //         imgs = imgs.Append($"D:\\att_faces\\s{i}\\{j}.pgm").ToArray();
            //         labels = labels.Append(0).ToArray();
            //     }
            // }

            var src = new Mat[imgs.Length];


            for (int i = 0; i < imgs.Length; i++)
            {
                var img = Cv2.ImRead(imgs[i]);
                Mat faceImg;
                if (i < 3)
                {
                    var face = DetectFaceHC(img);
                    faceImg = img.SubMat(face.Value);
                    Cv2.Resize(faceImg, faceImg, new Size(92, 112));
                    Cv2.CvtColor(faceImg, faceImg, ColorConversionCodes.BGR2GRAY);
                    Cv2.EqualizeHist(faceImg, faceImg);
                    //Cv2.ImShow("1", faceImg);
                }
                else
                {
                    faceImg = img;
                    Cv2.CvtColor(faceImg, faceImg, ColorConversionCodes.BGR2GRAY);
                    Cv2.Normalize(faceImg, faceImg, 0, 255);
                }


                src[i] = faceImg;
            }

            Cv2.NamedWindow("1");

            _subtractor = BackgroundSubtractorKNN.Create();
            _recognizer = LBPHFaceRecognizer.Create();
            _recognizer.Train(src, labels);


            _timer.Start();
        }

        private Mat bck = new Mat();

        private Mat GetFaceImg(Mat frame)
        {
            var face = DetectFaceHC(frame);
            if (face.HasValue && face.Value.Width > 0 && face.Value.Height > 0)
            {
                var faceImg = frame.SubMat(face.Value);
                Cv2.Resize(faceImg, faceImg, new Size(92, 112));
                Cv2.CvtColor(faceImg, faceImg, ColorConversionCodes.BGR2GRAY);
                Cv2.EqualizeHist(faceImg, faceImg);

                //frame.SetTo(new Scalar(0, 0, 0));

                //faceImg.Resize(frame.Width * frame.Height, new Scalar(255,255,255));

                // faceImg = faceImg.Resize(faceImg.Size());

                // for (int i = 0; i < face.Value.Width; i++)
                // {
                //     for (int j = 0; j < face.Value.Height; j++)
                //     {
                //         frame.Set(face.Value.TopLeft.X + i, face.Value.TopLeft.Y + j, 0);
                //     }
                // }

                using var without = new Mat();
                for (int i = 0; i < face.Value.Y + face.Value.Height; i++)
                {
                    without.PushBack(frame.Row.Get(i));
                }

                without.CopyTo(frame);

                for (int i = 0; i < face.Value.Width; i++)
                {
                    for (int j = 0; j < face.Value.Height; j++)
                    {
                        frame.Set(face.Value.Y + j, face.Value.X + i, 0);
                    }
                }

                if (bck.Empty())
                {
                    

                   
                    frame.CopyTo(bck);
                    Cv2.CvtColor(bck, bck, ColorConversionCodes.BGR2HSV);
                }


                Cv2.ImShow("1", faceImg);



                //frame.SetTo(new Scalar(0, 0, 0), faceImg);

                return faceImg;
            }

            return null;
        }


        private int p = 0;
        private Mat fgMask = new Mat();

        private void TimerOnTick(object? sender, EventArgs e)
        {
            using var frame = _capture.RetrieveMat();


            var faceImg = GetFaceImg(frame);
            // if (faceImg != null)
            // {
            //     _recognizer.Predict(faceImg, out var marek, out var conf);
            //     Debug.WriteLine($"{marek}, {conf}");
            //
            //
            //     if (p < 300)
            //     {
            //         _recognizer.Update(new[] {faceImg}, new[] {1});
            //         p++;
            //         DataContext = $"Learning: true {(p * 100 / 500.0f):F4}% Accur: {conf}";
            //     }
            //     else
            //     {
            //         DataContext = $"Learning: false Accur: {conf}";
            //
            //
            //         if (conf < 60.0f)
            //         {
            //             _recognizer.Update(new[] {faceImg}, new[] {1});
            //         }
            //     }
            // }

            if (!bck.Empty())
            {
                var h1 = new Mat();
                var h2 = new Mat();


                //Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2GRAY);
                //Cv2.CvtColor(bck, bck, ColorConversionCodes.BGR2GRAY);

                Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2HSV);

                Cv2.CalcHist(new []{frame}, new []{0,1,2}, mask: null, hist: h1, dims: 1, histSize: new []{180}, ranges:new []{new Rangef(0,180f), });
                Cv2.CalcHist(new[] { bck }, new[] { 0, 1, 2 }, null, hist: h2, dims: 1, histSize: new[] { 180 }, ranges: new[] { new Rangef(0, 180f), });

                DataContext = Cv2.CompareHist(h1, h2, HistCompMethods.Intersect);
            }

            image.Source = BitmapToImageSource(frame.ToBitmap());
            //Cv2.ImShow("2", fgMask);
        }

        private Rect? DetectFaceHC(Mat frame)
        {
            var faces = _face_cascade.DetectMultiScale(frame);

            return faces
                .FirstOrDefault(f => (f.Width * f.Height) == faces.Select(v => v.Width * v.Height).Max());
        }
    }
}