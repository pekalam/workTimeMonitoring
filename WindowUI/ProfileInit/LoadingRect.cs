using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WindowUI.ProfileInit
{
    public class LoadingRect : Image
    {
        public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register(
            "Thickness", typeof(int), typeof(LoadingRect), new PropertyMetadata(1, ThicknessChanged));

        public int Thickness
        {
            get { return (int)GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }

        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
            "Width", typeof(int), typeof(LoadingRect), new PropertyMetadata(1, WidthChanged));

        public int Width
        {
            get { return (int)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly DependencyProperty HeightProperty = DependencyProperty.Register(
            "Height", typeof(int), typeof(LoadingRect), new PropertyMetadata(1, HeightChanged));

        public int Height
        {
            get { return (int)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }


        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
            "Progress", typeof(int), typeof(LoadingRect), new PropertyMetadata(default(int), ProgressChanged));


        public int Progress
        {
            get { return (int)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        private static void ThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bar = (LoadingRect)d;
            bar.ChangeSz();
        }

        private static void WidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bar = (LoadingRect)d;
            bar.ChangeSz();
        }

        private static void HeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bar = (LoadingRect)d;
            bar.ChangeSz();
        }

        private static void ProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bar = (LoadingRect)d;
            bar.ChangeProgress((int)e.OldValue);
        }

        private readonly List<(int x, int y)[]> _border = new List<(int x, int y)[]>();

        public LoadingRect()
        {
        }

        private void ChangeSz()
        {
            if (Width == 0 || Height == 0)
            {
                return;
            }

            _border.Clear();
            var wb = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Bgra32, null);


            for (int i = Height - 1; i > 0; i--)
            {
                var b = new (int, int)[Thickness];
                for (int t = 0; t < Thickness; t++)
                {
                    b[t] = (t, i);
                }
                _border.Add(b);
            }
            for (int i = 0; i < Width; i++)
            {
                var b = new (int, int)[Thickness];
                for (int t = 0; t < Thickness; t++)
                {
                    b[t] = (i, t);
                }
                _border.Add(b);
            }
            for (int i = 1; i < Height; i++)
            {
                var b = new (int, int)[Thickness];
                for (int t = 0; t < Thickness; t++)
                {
                    b[t] = (Width - (t + 1), i);
                }
                _border.Add(b);
            }
            for (int i = Width - 1; i > 0; i--)
            {
                var b = new (int, int)[Thickness];
                for (int t = 0; t < Thickness; t++)
                {
                    b[t] = (i, Height - (t + 1));
                }
                _border.Add(b);
            }

            Source = wb;

            ChangeProgress(0);
        }

        private void ChangeProgress(int previous)
        {
            var wb = (Source as WriteableBitmap);
            var pixels = new byte[(int)wb.Width * (int)wb.Height * wb.Format.BitsPerPixel / 8];
            wb.CopyPixels(pixels, wb.PixelWidth * wb.Format.BitsPerPixel / 8, 0);

            var end = Progress * _border.Count / 100;
            byte[] color = new byte[] { 255, 255, 0, 255 };

            if (Progress == 0)
            {
                end = _border.Count;
                color = new byte[] { 255, 255, 0, 0 };
            }

            for (int i = 0; i < end; i++)
            {
                foreach (var p in _border[i])
                {
                    int pos = (p.x + p.y * wb.PixelWidth) * wb.Format.BitsPerPixel / 8;
                    pixels[pos] = color[0];
                    pixels[pos + 1] = color[1];
                    pixels[pos + 2] = color[2];
                    pixels[pos + 3] = color[3];
                }


            }

            int stride = wb.PixelWidth * wb.Format.BitsPerPixel / 8;
            wb.WritePixels(new Int32Rect(0, 0, (int)wb.Width, (int)wb.Height), pixels, stride, 0);
        }
    }
}