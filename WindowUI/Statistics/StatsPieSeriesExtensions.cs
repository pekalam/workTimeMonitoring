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
            if (label != null)
            {
                series.LabelPoint = _ => label;
            }
            series.Values = new ChartValues<int>(new int[] {events.Sum(e => e.MkEvent.TotalTime)});
            series.Title = "Mouse";
            series.DataLabels = true;
            return new[] {series};
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<KeyboardAction> events, string label = null)
        {
            var series = new PieSeries();
            series.Fill = Brushes.Coral;
            if (label != null)
            {
                series.LabelPoint = _ => label;
            }
            series.Values = new ChartValues<int>(new int[] {events.Sum(e => e.MkEvent.TotalTime)});
            series.Title = "Keyboard";
            series.DataLabels = true;
            return new[] {series};
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<UserWatchingScreen> events, string label = null)
        {
            var series = new PieSeries();
            series.Fill = Brushes.HotPink;
            if (label != null)
            {
                series.LabelPoint = _ => label;
            }
            series.Values = new ChartValues<long>(new long[] { events.Sum(e => e.TotalTimeMs) });
            series.Title = "Watching screen";
            series.DataLabels = true;
            return new[] { series };
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<FaceRecognitionFailure> events, string label = null)
        {
            var series = new PieSeries();
            series.Fill = Brushes.DarkGray;
            if (label != null)
            {
                series.LabelPoint = _ => label;
            }
            series.Values = new ChartValues<long>(new long[] { events.Sum(e => e.LengthMs) });
            series.Title = "Away";
            series.DataLabels = true;
            return new[] { series };
        }
    }
}