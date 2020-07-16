using System;
using ProgressApp.Core.Common;

namespace ProgressApp.Core {
public interface
IHasProgress {
    Progress? Progress { get; }
}

public interface 
IProgressObserver {
    void OnProgress(long value, long? targetValue, string message);
    void OnProgress(long deltaValue);
    void OnProgress(string message);
}

public class
ProgressObserver : IProgressObserver, IHasProgress {
    private readonly RateEstimator _rateEstimator = new RateEstimator();
    public Progress? Progress { get; private set; }

    public void
    OnProgress(long value, long? targetValue, string message = "") =>
        Progress = new Progress(value, targetValue, 0, TimeSpan.Zero, message);

    public void
    OnProgress(string message) => 
        Progress = Progress?.ReplaceMessage(message) ?? Progress.Empty.ReplaceMessage(message);

    public void
    OnProgress(long deltaValue) {
        deltaValue.VerifyNonNegative();
        Progress ??= Progress.Empty;

        var rate = _rateEstimator.GetCurrentRate(deltaValue);
        var newValue = Progress.Value + deltaValue;

        long timeLeft = 0;
        if (Progress.TargetValue != null && Progress.TargetValue.Value > newValue && rate > 0)
            timeLeft = (Progress.TargetValue.Value - newValue) / rate;

        Progress = new Progress(
            value: newValue,
            targetValue: Progress.TargetValue,
            rate: rate,
            timeLeft: timeLeft.Seconds(),
            Progress.Message);
    }

    public void 
    Reset() => Progress = null;
}

public class
Progress {
    public long Value { get; } 
    public long? TargetValue { get; } 
    public long Rate { get; }
    public TimeSpan TimeLeft { get; }
    public string Message { get; }

    public Progress(long value, long? targetValue, long rate, TimeSpan timeLeft, string message) {
        Value = value.VerifyNonNegative();
        TargetValue = targetValue?.VerifyNonNegative();
        Rate = rate.VerifyNonNegative();
        TimeLeft = timeLeft.VerifyNonNegative();
        Message = message;
    }

    public static readonly Progress 
    Empty = new Progress(0, null, 0, TimeSpan.Zero, string.Empty);

    public override string 
    ToString() => $"{Message}: {Rate}/s - {Value} of {TargetValue}, {TimeLeft.ToReadable()} left";
}

public static class
ProgressHelper {

    public static Progress
    ReplaceMessage(this Progress progress, string message) =>
        new Progress(
            value: progress.Value,
            targetValue: progress.TargetValue,
            rate: progress.Rate,
            timeLeft: progress.TimeLeft,
            message: message);
}

// ringbuffer, в конструкторе принимает интервал в секундах за который считается средняя скорость
public sealed class 
RateEstimator {
    private readonly long[] _rateArray;
    private int _oldestIndex;
    private int _intervalCount;
    private DateTime _oldestTime;

    public RateEstimator(int timeIntervalSeconds = 10) 
        : this(timeIntervalSeconds, DateTime.UtcNow) { }

    public RateEstimator(int timeIntervalSeconds, DateTime timeStamp) {
        timeIntervalSeconds.VerifyNonNegative();
        _rateArray = new long[timeIntervalSeconds];
        Reset(timeStamp);
    }

    public long 
    GetCurrentRate(long deltaValue = 0) => GetCurrentRate(deltaValue, DateTime.UtcNow);

    public long 
    GetCurrentRate(long deltaValue, DateTime timeStamp) {
        ClearOldData(timeStamp);
        Increment(deltaValue, timeStamp);
        return CalculateRate();
    
        void
        Increment(long value, DateTime now) {
            var secondsSinceOldest = (now - _oldestTime).Seconds;
            var currentIndex = (_oldestIndex + secondsSinceOldest) % _rateArray.Length;
            _rateArray[currentIndex] += value;
        }

        long
        CalculateRate() {
            long total = 0;
            for (var i = 0; i < _intervalCount; i++) {
                var index = (_oldestIndex + i) % _rateArray.Length;
                total += _rateArray[index];
            }
            return total / _intervalCount;
        }
    }

    private void 
    Reset(DateTime timestamp) {
        for (var i=0; i < _rateArray.Length; i++) {
            _rateArray[i] = 0;
        }
        _oldestIndex = 0;
        _intervalCount = 1;
        _oldestTime = timestamp;
    }

    private void 
    ClearOldData(DateTime timestamp) {
        var secondsSinceOldest = (timestamp - _oldestTime).Seconds;
        if (secondsSinceOldest < 0) {
            Reset(timestamp);
            return;
        }
        var index = secondsSinceOldest;
        if (index < _rateArray.Length) {
            _intervalCount = index + 1;
            return;
        }

        var extraIntervals = index - _rateArray.Length + 1;
        if (extraIntervals > _rateArray.Length) {
            Reset(timestamp);
            return;
        }

        _intervalCount = _rateArray.Length;
        for (var i = 0; i < extraIntervals; i++) {
            _rateArray[_oldestIndex] = 0;
            _oldestIndex = (_oldestIndex + 1) % _rateArray.Length;
            _oldestTime += TimeSpan.FromSeconds(1);
        }
    }
}
}
