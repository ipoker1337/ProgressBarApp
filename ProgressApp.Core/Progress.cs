using System;
using System.Diagnostics;
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
        Progress = Progress.Create(value, targetValue, message);

    public void
    OnProgress(string message) => 
        Progress = Progress?.WithMessage(message) ?? Progress.Create().WithMessage(message);

    public void
    OnProgress(long deltaValue) {
        deltaValue.VerifyNonNegative();
        var p = Progress ?? Progress.Create();
        var rate = _rateEstimator.GetCurrentRate(deltaValue);
        var newValue = p.Value + deltaValue;
        long timeLeft = 0;
        if (p.TargetValue != null && p.TargetValue.Value > newValue && rate > 0)
            timeLeft = (p.TargetValue.Value - newValue) / rate;
        Progress = p.Update(newValue, rate, timeLeft.Seconds());
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

    private Progress(long value, long? targetValue, long rate, TimeSpan timeLeft, string message) {
        Value = value.VerifyNonNegative();
        TargetValue = targetValue?.VerifyNonNegative();
        Rate = rate.VerifyNonNegative();
        TimeLeft = timeLeft.VerifyNonNegative();
        Message = message;
    }

    public static Progress
    Create() => new Progress(
        value: 0,
        targetValue: null,
        rate: 0,
        timeLeft: TimeSpan.Zero,
        message: string.Empty);

    public static Progress
    Create(long value, long? targetValue, string message) => new Progress(
        value: value,
        targetValue: targetValue,
        rate: 0,
        timeLeft: TimeSpan.Zero,
        message: message);

    public Progress 
    Update(long value, long rate, TimeSpan timeLeft) => new Progress(value, TargetValue, rate, timeLeft, Message);

    public Progress 
    WithMessage(string message) => new Progress(Value, TargetValue, Rate, TimeLeft, message);

    public Progress 
    WithRate(long rate) => new Progress(Value, TargetValue, rate.VerifyNonNegative(), TimeLeft, Message);

    public override string 
    ToString() => $"{Message}: {Rate} B/s - {Value} of {TargetValue}, {TimeLeft.ToReadable()} left";
}

// ringbuffer, в конструкторе принимает интервал в секундах за который считается средняя скорость
public sealed class 
RateEstimator {
    private readonly long[] _rateArray;
    private int _oldestIndex;
    private int _intervalCount;
    //TODO: TimeStamp должен быть в виде DateTime
    private long _oldestTime;

    public RateEstimator(int timeIntervalSeconds = 10) 
        : this(timeIntervalSeconds, Stopwatch.GetTimestamp()) { }

    public RateEstimator(int timeIntervalSeconds, long timeStamp) {
        timeIntervalSeconds.VerifyNonNegative();
        _rateArray = new long[timeIntervalSeconds];
        Reset(timeStamp);
    }

    public long 
    GetCurrentRate(long deltaValue = 0) => GetCurrentRate(deltaValue, Stopwatch.GetTimestamp());

    public long 
    GetCurrentRate(long deltaValue, long timeStamp) {
        ClearOldData(timeStamp);
        Increment(deltaValue, timeStamp);
        return CalculateRate();
    
        void
        Increment(long deltaValue, long timeStamp) {
            var secondsSinceOldest = new TimeSpan(timeStamp - _oldestTime).Seconds;
            var currentIndex = (_oldestIndex + secondsSinceOldest) % _rateArray.Length;
            _rateArray[currentIndex] += deltaValue;
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
    Reset(long timestamp) {
        for (var i=0; i < _rateArray.Length; i++) {
            _rateArray[i] = 0;
        }
        _oldestIndex = 0;
        _intervalCount = 1;
        _oldestTime = timestamp;
    }

    private void 
    ClearOldData(long timestamp) {
        var secondsSinceOldest = new TimeSpan(timestamp - _oldestTime).Seconds;
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
            _oldestTime += TicksFromSeconds(1);
        }

        long TicksFromSeconds(double seconds) => TimeSpan.FromSeconds(seconds).Ticks;
    }
}
}
