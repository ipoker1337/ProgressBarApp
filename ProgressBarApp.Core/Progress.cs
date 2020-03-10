using System;
using ProgressBarApp.Core.Common;

namespace ProgressBarApp.Core {

public interface 
IProgressProvider {
    event EventHandler<Progress>? ProgressChanged;
}

public readonly struct 
Progress {
    public long Value { get; } 
    public long Maximum { get; } 
    public double Speed { get; }
    public string Message { get; }
    public long ValueDelta { get; } 
    public TimeSpan TimeDelta { get; } 

    #region Constructors

    private Progress(long valueDelta, 
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

    public static Progress
    Create() => new Progress(
        valueDelta: 0,
        timeDelta: TimeSpan.Zero,
        value: 0,
        maximum: 0,
        speed: 0,
        message: string.Empty);

    public static Progress
    Create(string message) => new Progress(
        valueDelta: 0,
        timeDelta: TimeSpan.Zero,
        value: 0,
        maximum: 0,
        speed: 0,
        message: message);

    public static Progress
    Create(long value, long maximum, long valueDelta, TimeSpan timeDelta, string message = "") => new Progress(
        valueDelta: valueDelta,
        timeDelta: timeDelta,
        value: value,
        maximum: maximum,
        speed: (timeDelta.TotalMilliseconds > 0) ? valueDelta * 1000.0 / timeDelta.TotalMilliseconds : 0,
        message: message);

    #endregion

    #region Methods

    public Progress
    WithLastIncrement(long lastIncrement) 
        => Progress.Create(Value, Maximum, lastIncrement, TimeDelta, Message);

    public Progress
    WithValue(long value)
        => Progress.Create(value, Maximum, ValueDelta, TimeDelta, Message);

    public Progress
    WithMaximum(long maximum)
        => Progress.Create(Value, maximum, ValueDelta, TimeDelta, Message);

    public Progress
    WithTimeDelta(TimeSpan timeDelta)
        => Progress.Create(Value, Maximum, ValueDelta, timeDelta, Message);

    public Progress
    WithSpeed(double speed)
        => new Progress(
        valueDelta: ValueDelta,
        timeDelta: TimeDelta,
        value: Value,
        maximum: Maximum,
        speed: speed,
        message: Message);

    public Progress
    WithMessage(string message)
        => new Progress(
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
