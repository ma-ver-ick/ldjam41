using System;
using System.Diagnostics;

namespace ldjam41 {
    public class PausableStopwatch {
        private readonly Stopwatch _stopwatch;
        private long _pastDuration;

        public PausableStopwatch() {
            _stopwatch = Stopwatch.StartNew();
            IsRunning = true;
        }

        public void Stop() {
            _stopwatch.Stop();
            IsRunning = false;
            IsPaused = false;
        }

        public void Start() {
            _stopwatch.Start();
            IsRunning = true;
            IsPaused = false;
            _pastDuration = 0;
        }

        public void Pause() {
            _stopwatch.Stop();
            _pastDuration += _stopwatch.ElapsedMilliseconds;
            IsPaused = true;
        }

        public bool IsRunning { get; private set; }
        public bool IsPaused { get; private set; }

        public TimeSpan Elapsed => TimeSpan.FromMilliseconds(_stopwatch.ElapsedMilliseconds + _pastDuration);
        public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds + _pastDuration;
    }
}