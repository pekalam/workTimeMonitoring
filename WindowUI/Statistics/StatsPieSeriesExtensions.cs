using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Domain.WorkTimeAggregate.Events;
using LiveCharts;
using LiveCharts.Wpf;
using Brushes = System.Windows.Media.Brushes;

namespace WindowUI.Statistics
{
    public static class StatsPieSeriesExtensions
    {
        private static string TimeStr(long ms)
        {
            if (ms >= 36_000)
            {
                return (ms / 36_000).ToString("f") + " min";
            }
            else
            {
                return (ms / 1000).ToString("f") + " s";
            }
        }

        private static PieSeries CreateSeries(long ms, string label = null)
        {
            var series = new PieSeries();
            if (label != null)
            {
                series.LabelPoint = _ => label + "\n" + TimeStr(ms);
                series.DataLabels = true;
            }
            series.Values = new ChartValues<long>(new[] { ms });
            return series;
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<MouseAction> events, string label = null)
        {
            var ms = events.Sum(e => e.MkEvent.TotalTime);
            if (ms <= 0)
            {
                return Enumerable.Empty<PieSeries>();
            }
            var series = CreateSeries(ms, label);
            series.Fill = Brushes.BlueViolet;
            series.Title = "Mouse";
            return new[] {series};
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<KeyboardAction> events, string label = null)
        {
            var ms = events.Sum(e => e.MkEvent.TotalTime);
            if (ms <= 0)
            {
                return Enumerable.Empty<PieSeries>();
            }
            var series = CreateSeries(ms, label);
            series.Fill = Brushes.Coral;
            series.Title = "Keyboard";
            return new[] {series};
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<UserWatchingScreen> events, string label = null)
        {
            var ms = events.Sum(e => e.TotalTimeMs);
            if (ms <= 0)
            {
                return Enumerable.Empty<PieSeries>();
            }
            var series = CreateSeries(ms, label);
            series.Fill = Brushes.HotPink;
            series.Title = "Watching screen";
            return new[] { series };
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<FaceRecognitionFailure> events, string label = null)
        {
            var ms = events.Sum(e => e.LengthMs);
            if (ms <= 0)
            {
                return Enumerable.Empty<PieSeries>();
            }
            var series = CreateSeries(ms, label);
            series.Fill = Brushes.DarkGray;
            series.Title = "Away";
            return new[] { series };
        }
    }
}