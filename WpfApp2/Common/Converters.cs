using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using ProgressApp.Core;
using ProgressApp.Core.Common;

namespace WpfApp2.Common {

    // 288 КБ/с - 3,8 МБ из 58,6 МБ, Осталось 3 мин.
    // 4.9 MB/s - 24.7 MB of 58.6 MB, 6 secs left
    // 
    // var bestUnit = TargetValue.Bytes.DetectLargestUnit()
    // $"{Rate.Bytes}/s - Value.Bytes.To(bestUnit) of {TargetValue.Bytes} - {ETA.ToReadable()}"

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
ProgressToStringConverter : IValueConverter {
    public object 
    Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
        if (value is null)
            return string.Empty;
        if (!(value is Progress))
            throw new InvalidOperationException("The values[0] must be Progress");

        var p = (Progress) value;
        if (p.Value == 0)
            return string.Empty;

        var rate = p.Rate.FromBytes().ToReadable();

        if (!p.TargetValue.HasValue || p.TargetValue < p.Value)
            return $"{rate}/s - {p.Value.FromBytes().ToReadable()}";

        var targetValue = p.TargetValue?.FromBytes().ToReadable();
        var unit = p.TargetValue?.FromBytes().LargestUnit;
        var currentValue = p.Value.FromBytes().ToReadable(unit ?? ByteConverter.Unit.Bytes);

        return $"{rate}/s - {currentValue} of {targetValue}, {p.TimeLeft.ToReadable()} left";

    }

    public object 
    ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
    throw new NotImplementedException();
}

public class 
ProgressToIndeterminateConverter : IValueConverter {
    public object 
    Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
        if (value is null)
            return false;
        if (!(value is Progress))
            throw new InvalidOperationException("The target must be Progress");
        var progress = (Progress) value;
        return !progress.TargetValue.HasValue || progress.TargetValue < progress.Value;
    }

    public object 
    ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
}

