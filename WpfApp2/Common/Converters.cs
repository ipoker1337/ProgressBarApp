using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ProgressApp.Core;
using ProgressApp.Core.Common;

namespace WpfApp2.Common {

#region under development

public struct 
ByteConverter {
    public const long BytesInKilobyte = 1024;
    public const long BytesInMegabyte = 1048576;
    public const long BytesInGigabyte = 1073741824;

    public const string ByteSymbol = "B";
    public const string KilobyteSymbol = "KB";
    public const string MegabyteSymbol = "MB";
    public const string GigabyteSymbol = "GB";

    public enum Unit {
        Bytes = 0,
        Kilobytes = 1,
        Megabytes = 2,
        Gigabytes = 3,
    }

    public ByteConverter(double bytes) {
        ToBytes = bytes.VerifyNonNegative();
        ToKilobytes = bytes / BytesInKilobyte;
        ToMegabytes = bytes / BytesInMegabyte;
        ToGigabytes = bytes / BytesInGigabyte;
    }

    public static ByteConverter 
    FromBytes(long value) => new ByteConverter(value);

    public double ToBytes { get; }
    public double ToKilobytes { get; }
    public double ToMegabytes { get; }
    public double ToGigabytes { get; }

    public string ToReadable(Unit unit) {
        switch (unit) {
            case Unit.Gigabytes:
                return $"{ToGigabytes:N1} {GigabyteSymbol}";
            case Unit.Megabytes:
                return $"{ToMegabytes:N1} {MegabyteSymbol}";
            case Unit.Kilobytes:
                return $"{ToKilobytes:N1} {KilobyteSymbol}";
            default:
                return $"{ToBytes:N1} {ByteSymbol}";
        }
    }

    public string ToReadable() => ToReadable(LargestUnit);

    public Unit LargestUnit {
        get {
            if (Math.Abs(ToGigabytes) >= 1) 
                return Unit.Gigabytes;
            if (Math.Abs(ToMegabytes) >= 1)
                return Unit.Megabytes;
            if (Math.Abs(ToKilobytes) >= 1)
                return Unit.Kilobytes;
            return Unit.Bytes;
        }
    }
}

public static class 
Long {
    public static ByteConverter 
        FromBytes(this long value) => new ByteConverter((double)value);
}

#endregion


public class 
ProgressToTextConverter : IValueConverter {
    private DateTime _lastUpdate = DateTime.MinValue;

    private int _updateThresholdMs;
    public int UpdateThresholdMs {
        get => _updateThresholdMs;
        set => _updateThresholdMs = value.VerifyNonNegative();
    }

    public object 
    Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
        if (value is null)
            return string.Empty;
        var now = DateTime.Now;
        var elapsed = now - _lastUpdate;
        if (UpdateThresholdMs > elapsed.TotalMilliseconds)
            return Binding.DoNothing;
        _lastUpdate = now;
        return ProgressToText((Progress) value);

        static string 
        ProgressToText(Progress progress) {
            var rate = progress.Rate.FromBytes().ToReadable();
            if (!progress.TargetValue.HasValue || progress.TargetValue < progress.Value)
                return $"{rate}/s - {progress.Value.FromBytes().ToReadable()}";

            var targetValue = progress.TargetValue?.FromBytes().ToReadable();
            var unit = progress.TargetValue?.FromBytes().LargestUnit;
            var currentValue = progress.Value.FromBytes().ToReadable(unit ?? ByteConverter.Unit.Bytes);
            // 4.9 MB/s - 24.7 MB of 58.6 MB, 6 secs left
            return $"{rate}/s - {currentValue} of {targetValue}, {progress.TimeLeft.ToReadable()} left";
        }
    }

    public object 
    ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class 
ProgressToIndeterminateConverter : IValueConverter {
    public object 
    Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is null)
            return false;
        var progress = (Progress) value;
        return !progress.TargetValue.HasValue || progress.TargetValue < progress.Value;
    }

    public object 
    ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class
ProgressToValueConverter : IValueConverter {
    public object
    Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((Progress) value)?.Value ?? 0;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
    throw new NotImplementedException();
}

public class
NullToVisibilityConverter : IValueConverter {
    public object 
    Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        value == null ? Visibility.Collapsed : Visibility.Visible;

    public object 
    ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

}

