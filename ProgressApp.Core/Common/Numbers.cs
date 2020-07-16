using System;

namespace ProgressApp.Core.Common {
public static class 
Int {
    public static int
    VerifyInRange(this int target, int start, int end) {
        if (target >= start && target <= end)
            return target;
        throw new ArgumentOutOfRangeException();
    }

    public static int
    VerifyGreaterThanOrEqual(this int left, int right) {
        if (left < right)
            throw new ArgumentOutOfRangeException($"Cannot be less than {right}");
        return left;
    }

    public static int
    VerifyNonNegative(this int value) => 
        value.VerifyGreaterThanOrEqual(0);

    public static TimeSpan
    Hours(this int hours) => TimeSpan.FromHours(hours);

    public static TimeSpan 
    Minutes(this int minutes) => TimeSpan.FromMinutes(minutes);

    public static TimeSpan
    Seconds(this int seconds) => TimeSpan.FromSeconds(seconds);

    public static TimeSpan 
    Milliseconds(this int milliseconds) => TimeSpan.FromMilliseconds(milliseconds);
}

public static class 
Long {
    public static long
    VerifyGreaterThanOrEqual(this long left, long right) {
        if (left < right)
            throw new ArgumentOutOfRangeException($"Cannot be less than {right}");
        return left;
    }

    public static long
    VerifyNonNegative(this long value) {
        value.VerifyGreaterThanOrEqual(0);
        return value;
    }

    public static long
    VerifyGreaterZero(this long value) {
        value.VerifyGreaterThanOrEqual(1);
        return value;
    }

    public static long 
    VerifyInRange(this long target, long start, long end) {
        if (target >= start && target <= end)
            return target;
        throw new ArgumentOutOfRangeException();
    }

    public static TimeSpan 
    Seconds(this long seconds) => TimeSpan.FromSeconds(seconds);

    public static ByteSize 
    FromBytes(this long value) => ByteSize.FromBytes(value);
}

public static class 
Double {
    public static double
    VerifyNonNegative(this double value) {
        if (value < 0)
            throw new ArgumentOutOfRangeException($"Cannot be less than 0");
        return value;
    }
}
}

