using System;

namespace ProgressApp.Core.Common {
public static class
Time {
    private static readonly TimeSpan OneMinute = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan OneHour = TimeSpan.FromHours(1);
    private static readonly TimeSpan OneDay = TimeSpan.FromDays(1);

    public static TimeSpan
    VerifyNonNegative(this TimeSpan value) {
        if (value < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(value), "TimeSpan cannot be negative");
        return value;
    }

    public static TimeSpan
    VerifyGreaterZero(this TimeSpan value) {
        if (value <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(value), "TimeSpan should be greater zero");
        return value;
    }

    // under development
    public static string 
    ToReadable(this TimeSpan value) {
        if (value < TimeSpan.Zero)
            return "0 sec";
        if (value < OneMinute)
            return value.Seconds == 1 ? "1 sec" : $"{value.Seconds} secs";
        if (value < OneHour) 
            return value.Minutes == 1 ? "1 min" : $"{value.Minutes} mins";
        return value.Hours == 1 ? "1 hour" : $"{value.Hours} hours";
    }

}
}
