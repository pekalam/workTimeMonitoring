using Domain.WorkTimeAggregate.Events;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using Brushes = System.Windows.Media.Brushes;

namespace WindowUI.Statistics
{
    public static class StatsPieSeriesExtensions
    {
        private const int MsThreshold = 1000;
        private const int d = 60_000 * 60 * 24;
        private const int h = 60_000 * 60;

        private static string TimeStr(long ms)
        {
            string part(long t)
            {
                return t < 10 ? "0" + t : t.ToString();
            }

            if (ms >= d)
            {
                return ms / (d) +
                       (ms % (d) > 0 ? ":" + part(ms % d / h) : "") + " d";
            }
            if (ms >= h)
            {
                return ms / (h) + (ms % (h) > 0 ? ":" + part(ms % h / 60_000) : "") + " h";
            }
            if (ms >= 60_000)
            {
                return ms / 60_000 + (ms % 60_000 > 0 ? " m " + part(ms % 60_000 / 1000) + " s" : " m");
            }
            return (ms / 1000.0).ToString("f") + " s";
        }

        private static PieSeries CreateSeries(long ms, string label = "", bool newLine = false)
        {
            var series = new PieSeries();
            series.LabelPoint = _ => label + (newLine ? "\n" : String.Empty) + TimeStr(ms);
            series.DataLabels = true;
            series.Values = new ChartValues<long>(new[] { ms });
            return series;
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<MouseAction> events, string label = "", bool newLine = false)
        {
            var ms = events.Sum(e => e.MkEvent.TotalTime);
            if (ms <= MsThreshold)
            {
                return Enumerable.Empty<PieSeries>();
            }
            var series = CreateSeries(ms, label, newLine);
            series.Fill = Brushes.DeepSkyBlue;
            series.Title = "Mouse";
            return new[] {series};
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<KeyboardAction> events, string label = "", bool newLine = false)
        {
            var ms = events.Sum(e => e.MkEvent.TotalTime);
            if (ms <= MsThreshold)
            {
                return Enumerable.Empty<PieSeries>();
            }
            var series = CreateSeries(ms, label, newLine);
            series.Fill = Brushes.Coral;
            series.Title = "Keyboard";
            return new[] {series};
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<UserWatchingScreen> events, string label = "", bool newLine = false)
        {
            var ms = events.Sum(e => e.TotalTimeMs);
            if (ms <= MsThreshold)
            {
                return Enumerable.Empty<PieSeries>();
            }
            var series = CreateSeries(ms, label, newLine);
            series.Fill = Brushes.HotPink;
            series.Title = "Watching screen";
            return new[] { series };
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<FaceRecognitionFailure> events, string label = "", bool newLine = false)
        {
            var ms = events.Sum(e => e.LengthMs);
            if (ms <= MsThreshold)
            {
                return Enumerable.Empty<PieSeries>();
            }
            var series = CreateSeries(ms, label, newLine);
            series.Fill = Brushes.DarkGray;
            series.Title = "Away";
            return new[] { series };
        }
    }
}