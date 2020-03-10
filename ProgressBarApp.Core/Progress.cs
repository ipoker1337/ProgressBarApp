using System;
using ProgressBarApp.Core.Common;

namespace ProgressBarApp.Core {

public interface 
IProgressProvider {
    event EventHandler<ProgressInfo>? ProgressChanged;
}

public readonly struct 
ProgressInfo {
    public long Value { get; } // Index, ProgressIndex, CurrentIndex
    public long Maximum { get; } // EndIndex
    public double Speed { get; }
    public string Message { get; }
    public long ValueDelta { get; } //LastIndexIncrement, valueDelta, IncrementValue
    public TimeSpan TimeDelta { get; } //TimeDuration, TimeSpan, LastTimeElapsed

    #region Constructors

    private ProgressInfo(long valueDelta, 
                         TimeSpan timeDelta, 
                         long value, 
                         long maximum, 
                         double speed, 
                         string message = "") {
        ValueDelta = valueDelta.VerifyNonNegative();
        TimeDelta = timeDelta.VerifyNonNegative();
        Value = value.VerifyNonNegative();
        Maximum = maximum.VerifyNonNegative();
        Speed = speed.VerifyNonNegative();
        Message = message;
    }

    #endregion

    #region Static methods

    public static ProgressInfo
    Create() => new ProgressInfo(
        valueDelta: 0,
        timeDelta: TimeSpan.Zero,
        value: 0,
        maximum: 0,
        speed: 0,
        message: string.Empty);

    public static ProgressInfo
    Create(string message) => new ProgressInfo(
        valueDelta: 0,
        timeDelta: TimeSpan.Zero,
        value: 0,
        maximum: 0,
        speed: 0,
        message: message);

    public static ProgressInfo
    Create(long value, long maximum, long valueDelta, TimeSpan timeDelta, string message = "") => new ProgressInfo(
        valueDelta: valueDelta,
        timeDelta: timeDelta,
        value: value,
        maximum: maximum,
        speed: (timeDelta.TotalMilliseconds > 0) ? valueDelta * 1000.0 / timeDelta.TotalMilliseconds : 0,
        message: message);

    #endregion

    #region Methods

    public ProgressInfo
    WithLastIncrement(long lastIncrement) 
        => ProgressInfo.Create(Value, Maximum, lastIncrement, TimeDelta, Message);

    public ProgressInfo
    WithValue(long value)
        => ProgressInfo.Create(value, Maximum, ValueDelta, TimeDelta, Message);

    public ProgressInfo
    WithMaximum(long maximum)
        => ProgressInfo.Create(Value, maximum, ValueDelta, TimeDelta, Message);

    public ProgressInfo
    WithTimeDelta(TimeSpan timeDelta)
        => ProgressInfo.Create(Value, Maximum, ValueDelta, timeDelta, Message);

    public ProgressInfo
    WithSpeed(double speed)
        => new ProgressInfo(
        valueDelta: ValueDelta,
        timeDelta: TimeDelta,
        value: Value,
        maximum: Maximum,
        speed: speed,
        message: Message);

    public ProgressInfo
    WithMessage(string message)
        => new ProgressInfo(
        valueDelta: ValueDelta,
        timeDelta: TimeDelta,
        value: Value,
        maximum: Maximum,
        speed: Speed,
        message: message);

    public override string 
    ToString() => $"{(Maximum > 0 ? (double)Value / Maximum : 0.0):P} ({Value} / {Maximum}), {Value} MB | {Speed} MB/s";

    #endregion

}
}
