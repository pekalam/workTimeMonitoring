using System.Collections.Generic;
using System.Linq;
using Domain.WorkTimeAggregate.Events;
using LiveCharts;
using LiveCharts.Wpf;

namespace WindowUI.Statistics
{
    internal static class SeriesConversionExtensions
    {
        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<MouseAction> events)
        {
            var series = new PieSeries();
            series.Values = new ChartValues<int>(new int[] {events.Sum(e => e.MkEvent.TotalTime)});
            return new[] { series };
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<KeyboardAction> events)
        {
            var series = new PieSeries();
            series.Values = new ChartValues<int>(new int[] { events.Sum(e => e.MkEvent.TotalTime) });
            return new []{series};
        }
    }
}