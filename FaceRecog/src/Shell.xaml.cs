using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using Infrastructure;
using Infrastructure.WorkTime;
using MahApps.Metro.Controls;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Face;
using Size = OpenCvSharp.Size;
using Window = System.Windows.Window;

namespace FaceRecog
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : MetroWindow
    {
        private FaceRecognizer _recognizer;
        private BackgroundSubtractor _subtractor;

        private CaptureService c = new CaptureService();

        private WMonitorAlghorithm _alghorithm = new WMonitorAlghorithm();

        public Shell()
        {
            InitializeComponent();
        }

        private void Handler(InitFaceProgressArgs obj)
        {
            Debug.WriteLine(obj.ProgressPercentage);
            Debug.WriteLine(obj.ProgressState.ToString());
            if (obj.FaceRect.HasValue && obj.Frame != null)
            Cv2.Rectangle(obj.Frame, obj.FaceRect.Value, Scalar.Coral);
            if(obj.Frame != null)
            image.Source = BitmapToImageSource(obj.Frame.ToBitmap());
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            //image.Source = BitmapToImageSource(c.CaptureSingleFrame().ToBitmap());
            _alghorithm.Start(new Progress<InitFaceProgressArgs>(Handler));
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

        private async void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}