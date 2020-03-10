using System;

namespace ProgressBarApp.Core.Common {
    
public static class 
Numbers {
    // INT
    public static int
    VerifyInRange(this int target, int start, int end) {
        if (target >= start && target <= end)
            return target;
        throw new ArgumentOutOfRangeException();
    }

    // LONG
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

    // DOUBLE
    public static double
    VerifyNonNegative(this double value) {
        if (value < 0)
            throw new ArgumentOutOfRangeException($"Cannot be less than 0");
        return value;
    }

    // TimeSpan
    public static TimeSpan
    VerifyNonNegative(this TimeSpan value) {
        if (value < TimeSpan.Zero)
            throw new ArithmeticException("Cannot be negative");
        return value;
    }

}
}
