using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Infrastructure.Domain;
using Infrastructure.WorkTimeAlg;
using Xunit;

namespace UnitTests
{

    public class MouseKeyboardEventReducerServiceTest
    {
        private const int WindowSz = 60_000;

        private MouseKeyboardEventBuilder _service;

        public MouseKeyboardEventReducerServiceTest()
        {
            _service= new MouseKeyboardEventBuilder( 60_000, 1000);
        }


        [Fact]
        public void g()
        {
            MouseKeyboardEvent empty = null;
            var now = DateTime.Now;

            var e1 = new MonitorEvent()
            {
                EventStart = now,
                TotalTimeMs = 1200
            };

            _service.AddEvent(e1, out empty).Should().BeFalse();
            empty.Should().BeNull();

            var e2 = new MonitorEvent()
            {
                EventStart = now.AddMilliseconds(2000),
                TotalTimeMs = 2000
            };

            _service.AddEvent(e2, out empty).Should().BeFalse();
            empty.Should().BeNull();


            var created = _service.Flush();
            created.Should().NotBeNull();

            created.EventsTimeline.Should().BeEquivalentTo(new[] { 0, 1200, 2000, 4000});
            created.TotalTime.Should().Be(1200 + 2000);
            created.End.Should().Be(e1.EventStart.AddMilliseconds(4000));
        }

        [Fact]
        public void g2()
        {
            MouseKeyboardEvent empty = null;
            var now = DateTime.Now;

            var e1 = new MonitorEvent()
            {
                EventStart = now,
                TotalTimeMs = 500
            };

            _service.AddEvent(e1, out empty).Should().BeFalse();
            empty.Should().BeNull();

            now = now.AddMilliseconds(750);

            var e2 = new MonitorEvent()
            {
                EventStart = now,
                TotalTimeMs = 2000
            };

            _service.AddEvent(e2, out empty).Should().BeFalse();
            empty.Should().BeNull();


            var created = _service.Flush();
            created.Should().NotBeNull();

            created.EventsTimeline.Should().BeEquivalentTo(new[] { 0, 500, 750, 2750 });
            created.TotalTime.Should().Be(2500);
            created.End.Should().Be(e1.EventStart.AddMilliseconds(2750));
        }


        [Fact]
        public void Event_behind_window_forces_build()
        {
            MouseKeyboardEvent empty = null;
            var now = DateTime.Now;

            var e1 = new MonitorEvent()
            {
                EventStart = now,
                TotalTimeMs = 1200
            };

            _service.AddEvent(e1, out empty).Should().BeFalse();
            empty.Should().BeNull();

            now = now.AddMilliseconds(WindowSz * 2);

            var e2 = new MonitorEvent()
            {
                EventStart = now.AddMilliseconds(2000),
                TotalTimeMs = 2000
            };

            _service.AddEvent(e2, out var created).Should().BeTrue();
            created.Should().NotBeNull();

            created.EventsTimeline.Should().BeEquivalentTo(new[] { 0, 1200 });
            created.TotalTime.Should().Be(1200);
            created.End.Should().Be(e1.EventStart.AddMilliseconds(1200));
        }


        [Fact]
        public void x()
        {
            MouseKeyboardEvent empty = null;
            var now = DateTime.Now;

            var e1 = new MonitorEvent()
            {
                EventStart = now,
                TotalTimeMs = 1200
            };

            _service.AddEvent(e1, out empty).Should().BeFalse();
            empty.Should().BeNull();

            now = now.AddMilliseconds(WindowSz - 1000);

            var e2 = new MonitorEvent()
            {
                EventStart = now,
                TotalTimeMs = 2000
            };

            _service.AddEvent(e2, out empty).Should().BeFalse();
            empty.Should().BeNull();

            var created = _service.Flush();

            created.EventsTimeline.Should().BeEquivalentTo(new[] { 0, 1200, WindowSz - 1000, WindowSz - 1000 + 2000 });
            created.End.Should().Be(e1.EventStart.AddMilliseconds(WindowSz - 1000 + 2000));
        }


        [Fact]
        public void f()
        {
            MouseKeyboardEvent empty = null;
            var now = DateTime.Now;

            var e1 = new MonitorEvent()
            {
                EventStart = now,
                TotalTimeMs = 250
            };

            _service.AddEvent(e1, out empty).Should().BeFalse();
            empty.Should().BeNull();

            var e2 = new MonitorEvent()
            {
                EventStart = now.AddMilliseconds(500),
                TotalTimeMs = 250
            };

            _service.AddEvent(e2, out empty).Should().BeFalse();
            empty.Should().BeNull();

            var e3 = new MonitorEvent()
            {
                //EventStart = now.AddMilliseconds(500),
                EventStart = now.AddMilliseconds(1000),
                TotalTimeMs = 250
            };
            _service.AddEvent(e3, out empty).Should().BeFalse();
            empty.Should().BeNull();


            now = now.AddMilliseconds(WindowSz * 2);

            var e4 = new MonitorEvent()
            {
                EventStart = now.AddMilliseconds(500),
                TotalTimeMs = 250
            };

            _service.AddEvent(e4, out var created).Should().BeTrue();
            created.Should().NotBeNull();


            created.EventsTimeline.Should().BeEquivalentTo(new[] { 0, 500, 1000, 1250 });
            created.TotalTime.Should().Be(750);
            created.End.Should().Be(e1.EventStart.AddMilliseconds(1250));
        }
    }
}
