using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace WpfApp2.Infrastructure {

    public class ThrottleDispatcher {
        private DispatcherTimer? timer;
        private DateTime timerStarted { get; set; } = DateTime.UtcNow.AddYears(-1);
        private Dispatcher _dispatcher;

        public ThrottleDispatcher() {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public void Throttle(double interval, Action<object?> action,
            object? param = null,
            DispatcherPriority priority = DispatcherPriority.ApplicationIdle) {

            var innerInterval = timer?.Interval.TotalMilliseconds ?? interval;

            timer?.Stop();
            timer = null;

            var curTime = DateTime.UtcNow;
            var diff = curTime.Subtract(timerStarted);

            var debugInterval = innerInterval;

            if (diff.TotalMilliseconds < innerInterval)
                innerInterval -= diff.TotalMilliseconds;

            //Debug.WriteLine($"[{diff.TotalMilliseconds}ms] .Throttle({debugInterval}, ...)  timer = new timer(interval = {innerInterval}ms)");

            timer = new DispatcherTimer(priority, _dispatcher);
            timer.Interval = TimeSpan.FromMilliseconds(innerInterval);
            timer.Tick += (sender, args) => {
                if (timer == null)
                    return;

                timer?.Stop();
                timer = null;
                action.Invoke(param);
            };
            timer.Start();
            timerStarted = curTime;
        }
    }
}

