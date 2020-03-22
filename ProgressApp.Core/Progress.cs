using System;
using System.Diagnostics;
using System.Threading;
using ProgressApp.Core.Common;

namespace ProgressApp.Core {

public interface 
IProgressProvider {
    Progress? Progress { get; }
}

public class
ProgressProvider : IProgressProvider {
    private readonly RateEstimator _rateEstimator;

    protected ProgressProvider() {
        _rateEstimator = new RateEstimator(10);
    }

    public Progress? Progress { get; private set; }

//Under development

    protected void
    Report(long value, long? targetValue, string message = "") =>
    Progress = Core.Progress.Create(value, targetValue, message);


    protected void
    Report(long deltaValue) => CalculateAndPostUpdate(deltaValue);

    protected void
    Report(string message) =>
    Progress = Progress?.WithMessage(message) ?? Core.Progress.Create().WithMessage(message);


    private void
    CalculateAndPostUpdate(long deltaValue = 0) {

        var p = Progress ?? Core.Progress.Create();
        _rateEstimator.Increment(deltaValue);
        var rate = _rateEstimator.GetCurrentRate();
        var newValue = p.Value + deltaValue.VerifyNonNegative();
        long timeLeft = 0;
        if (p.TargetValue != null)
            timeLeft = rate > 0 ? (p.TargetValue.Value - newValue) / rate : 0;
        Progress = p.Update(newValue, rate, timeLeft.Seconds());
    }
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
        TimeLeft = timeLeft;
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

#region under development
// Есть подоздрение, что это не functional first стиль :) 
// Это временный вариант

// ringbuffer, в конструкторе принимает интервал в секундах за который считается средняя скорость
public sealed class 
RateEstimator {
    private readonly long[] _array;
    private int _oldestIndex;
    private int _intervalCount;
    private long _oldestTime;

    public RateEstimator(int timeIntervalSec) 
        : this(timeIntervalSec, Stopwatch.GetTimestamp()) { }

    public RateEstimator(int timeIntervalSec, long timestamp) {
        timeIntervalSec.VerifyNonNegative();
        _array = new long[timeIntervalSec];
        Reset(timestamp);
    }

    public void Increment(long value) => Increment(value, Now);
    public void Increment(long value, long timestamp) {
        ClearOld(timestamp);
        var secondsSinceOldest = new TimeSpan(timestamp - _oldestTime).Seconds;
        var currentIndex = (_oldestIndex + secondsSinceOldest) % _array.Length;
        _array[currentIndex] += value;
    }

    public long GetCurrentRate() => GetCurrentRate(Now);

    public long GetCurrentRate(long timestamp) {
        ClearOld(timestamp);
        long total = 0;
        for (var i = 0; i < _intervalCount; i++) {
            var index = (_oldestIndex + i) % _array.Length;
            total += _array[index];
        }
        return total / _intervalCount;
    }

    private void ClearOld(long timestamp) {
        var secondsSinceOldest = new TimeSpan(timestamp - _oldestTime).Seconds;
        if (secondsSinceOldest < 0) {
            Reset(timestamp);
            return;
        }
        var index = secondsSinceOldest;
        if (index < _array.Length) {
            _intervalCount = index + 1;
            return;
        }

        var extraIntervals = index - _array.Length + 1;
        if (extraIntervals > _array.Length) {
            Reset(timestamp);
            return;
        }

        _intervalCount = _array.Length;
        for (var i = 0; i < extraIntervals; i++) {
            _array[_oldestIndex] = 0;
            _oldestIndex = (_oldestIndex + 1) % _array.Length;
            _oldestTime += TicksFromSeconds(1);
        }
    }

    private long Now => Stopwatch.GetTimestamp();

    private static long TicksFromSeconds(double seconds) => 
        (long)(Stopwatch.Frequency * seconds);

    private void Reset(long timestamp) {
        for (var i=0; i < _array.Length; i++) {
            _array[i] = 0;
        }
        _oldestIndex = 0;
        _intervalCount = 1;
        _oldestTime = timestamp;
    }
}
#endregion

}
