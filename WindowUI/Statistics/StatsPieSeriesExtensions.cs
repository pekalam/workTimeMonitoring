using System.Collections.Generic;
using System.Linq;
using Domain.WorkTimeAggregate.Events;
using LiveCharts;
using LiveCharts.Wpf;

namespace WindowUI.Statistics
{
    public static class StatsPieSeriesExtensions
    {
        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<MouseAction> events)
        {
            var series = new PieSeries();
            series.Values = new ChartValues<int>(new int[] {events.Sum(e => e.MkEvent.TotalTime)});
            series.Title = "Mouse";
            return new[] {series};
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<KeyboardAction> events)
        {
            var series = new PieSeries();
            series.Values = new ChartValues<int>(new int[] {events.Sum(e => e.MkEvent.TotalTime)});
            series.Title = "Keyboard";
            return new[] {series};
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<UserWatchingScreen> events)
        {
            var series = new PieSeries();
            series.Values = new ChartValues<long>(new long[] { events.Sum(e => e.TotalTimeMs) });
            series.Title = "Watching screen";
            return new[] { series };
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<FaceRecognitionFailure> events)
        {
            var series = new PieSeries();
            series.Values = new ChartValues<long>(new long[] { events.Sum(e => e.LengthMs) });
            series.Title = "Away";
            return new[] { series };
        }
    }
}