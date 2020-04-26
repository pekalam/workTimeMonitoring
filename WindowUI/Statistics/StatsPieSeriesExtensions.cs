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
        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<MouseAction> events, string label = null)
        {
            var series = new PieSeries();
            series.Fill = Brushes.BlueViolet;
            var ms = events.Sum(e => e.MkEvent.TotalTime);
            if (label != null)
            {
                series.LabelPoint = _ => label;
                series.DataLabels = true;
            }
            series.Values = new ChartValues<int>(new int[] {ms});
            series.Title = "Mouse";
            return new[] {series};
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<KeyboardAction> events, string label = null)
        {
            var series = new PieSeries();
            series.Fill = Brushes.Coral;
            var ms = events.Sum(e => e.MkEvent.TotalTime);
            if (label != null)
            {
                series.LabelPoint = _ => label;
                series.DataLabels = true;
            }
            series.Values = new ChartValues<int>(new int[] {ms});
            series.Title = "Keyboard";
            return new[] {series};
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<UserWatchingScreen> events, string label = null)
        {
            var series = new PieSeries();
            series.Fill = Brushes.HotPink;
            var ms = events.Sum(e => e.TotalTimeMs);
            if (label != null)
            {
                series.LabelPoint = _ => label;
                series.DataLabels = true;
            }
            series.Values = new ChartValues<long>(new long[] { ms });
            series.Title = "Watching screen";
            return new[] { series };
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<FaceRecognitionFailure> events, string label = null)
        {
            var series = new PieSeries();
            series.Fill = Brushes.DarkGray;
            var ms = events.Sum(e => e.LengthMs);
            if (label != null)
            {
                series.LabelPoint = _ => label;
                series.DataLabels = true;
            }
            series.Values = new ChartValues<long>(new long[] { ms });
            series.Title = "Away";
            return new[] { series };
        }
    }
}